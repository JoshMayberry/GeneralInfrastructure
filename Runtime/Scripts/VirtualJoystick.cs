
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;
using jmayberry.CustomAttributes;

namespace jmayberry.GeneralInfrastructure.InputControls {

	/**
	 * Inspiration Sources:
	 *  - UnityEngine.InputSystem.OnScreen.OnScreenButton
	 *  - https://www.youtube.com/watch?v=MKnLPA5hnPA
	 *  - https://assetstore.unity.com/publishers/32730
	 */
	[DisallowMultipleComponent]
	[RequireComponent(typeof(RectTransform))]
	[AddComponentMenu("Input/VirtualJoystick")]
	public class VirtualJoystick : OnScreenControl {
		[Header("Refrences")]
		[Required] [SerializeField] private RectTransform borderRect;
        [Required] [SerializeField] private RectTransform knobRect;
		private RectTransform rect;

		private Finger movementFinger;
		private Canvas canvas;
		private Camera cam;

		[Header("Joystick")]
		[SerializeField] private float deadZone = 0.2f;
		[SerializeField] private float movementRange = 1;
		[SerializeField] private float dynamicMovementThreshold = 1;
		private Vector2 fixedPosition;

		public enum JoystickType { Fixed, Floating, Dynamic }
		[SerializeField] private JoystickType joystickType = JoystickType.Floating;

		public enum AxisOptions { Both, Horizontal, Vertical }
		[SerializeField] private AxisOptions axisOptions = AxisOptions.Both;

		public enum SideOptions { Left, Right }
		[SerializeField] private SideOptions side = SideOptions.Left;
		[SerializeField] private float sideThreshold = 0.5f;

		[Header("Output")]
		[Readonly] public Vector2 input;
		public float Horizontal { get { return input.x; } }
		public float Vertical { get { return input.y; } }

        [Required] [InputControl(layout = "Vector2")][SerializeField] private string m_ControlPath;
		protected override string controlPathInternal {
			get => this.m_ControlPath;
			set => this.m_ControlPath = value;
		}

		private void Awake() {
			this.rect = GetComponent<RectTransform>();
			this.canvas = GetComponentInParent<Canvas>();
			if (this.canvas == null) {
				Debug.LogError("The FloatingJoystick is not placed inside a canvas");
			}

			if (this.canvas.renderMode == RenderMode.ScreenSpaceCamera) {
				this.cam = this.canvas.worldCamera;
			}

			this.Reset();
		}

		protected override void OnEnable() {
			base.OnEnable();
			EnhancedTouchSupport.Enable();
			ETouch.Touch.onFingerDown += HandleFingerDown;
			ETouch.Touch.onFingerUp += HandleFingerUp;
			ETouch.Touch.onFingerMove += HandleFingerMove;
		}

		protected override void OnDisable() {
			base.OnDisable();
			ETouch.Touch.onFingerDown -= HandleFingerDown;
			ETouch.Touch.onFingerUp -= HandleFingerUp;
			ETouch.Touch.onFingerMove -= HandleFingerMove;
			EnhancedTouchSupport.Disable();
		}

		private void Reset() {
			Vector2 center = new Vector2(0.5f, 0.5f);
			this.borderRect.pivot = center;
			this.knobRect.anchorMin = center;
			this.knobRect.anchorMax = center;
			this.knobRect.pivot = center;

			this.knobRect.anchoredPosition = Vector2.zero;
			this.input = Vector2.zero;

			this.movementFinger = null;

			if (joystickType != JoystickType.Fixed) {
				this.borderRect.gameObject.SetActive(false);
			}
		}

		public void SetMode(JoystickType joystickType) {
			this.joystickType = joystickType;
			if (joystickType != JoystickType.Fixed) {
				this.borderRect.gameObject.SetActive(false);
				return;
			}

			this.borderRect.anchoredPosition = fixedPosition;
			this.borderRect.gameObject.SetActive(true);
		}

		private void HandleFingerDown(Finger touchedFinger) {
			if (movementFinger != null) {
				return; // We are already tracking a finger
			}

			switch (this.side) {
				case SideOptions.Left:
					if (touchedFinger.screenPosition.x > Screen.width * this.sideThreshold) {
						return; // Wrong side of the screen
					}
					break;

				case SideOptions.Right:
					if (touchedFinger.screenPosition.x <= Screen.width * this.sideThreshold) {
						return; // Wrong side of the screen
					}
					break;

				default:
					throw new Exception($"Unknown side '{this.side}'");
			}

			this.movementFinger = touchedFinger;
			input = Vector2.zero;

			if (joystickType == JoystickType.Fixed) {
				return;
			}

			this.borderRect.anchoredPosition = ScreenPointToAnchoredPosition(touchedFinger.screenPosition);
			this.borderRect.gameObject.SetActive(true);
		}

		private void HandleFingerUp(Finger lostFinger) {
			if (lostFinger != movementFinger) {
				return;
			}

			SendValueToControl(Vector2.zero);
			Reset();
		}

		private void HandleFingerMove(Finger movedFinger) {
			if (movedFinger != movementFinger) {
				return;
			}

			// Calculate Input
			ETouch.Touch currentTouch = movedFinger.currentTouch;
			Vector2 position = RectTransformUtility.WorldToScreenPoint(this.cam, this.borderRect.position);
			Vector2 radius = this.borderRect.sizeDelta / 2;
			this.input = (currentTouch.screenPosition - position) / (radius * this.canvas.scaleFactor);

			// Format Input
			if (axisOptions == AxisOptions.Horizontal) {
				this.input = new Vector2(this.input.x, 0f);
			}
			else if (axisOptions == AxisOptions.Vertical) {
				this.input = new Vector2(0f, this.input.y);
			}

			if (joystickType == JoystickType.Dynamic && this.input.magnitude > this.dynamicMovementThreshold) {
				Vector2 difference = this.input.normalized * (this.input.magnitude - this.dynamicMovementThreshold) * radius;
				this.borderRect.anchoredPosition += difference;
			}

			if (this.input.magnitude <= this.deadZone) {
				this.input = Vector2.zero;
			}
			else if (this.input.magnitude > 1) {
				this.input = input.normalized;
			}

			// Move Knob
			this.knobRect.anchoredPosition = this.input * radius * this.movementRange;
			SendValueToControl(this.input);
		}

		protected Vector2 ScreenPointToAnchoredPosition(Vector2 screenPosition) {
			Vector2 localPoint;
			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(this.rect, screenPosition, this.cam, out localPoint)) {
				return Vector2.zero;
			}

			Vector2 pivotOffset = this.rect.pivot * this.rect.sizeDelta;
			return localPoint - (this.borderRect.anchorMax * this.rect.sizeDelta) + pivotOffset;
		}
	}
}
