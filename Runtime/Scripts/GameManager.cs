using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
using Cinemachine;
using jmayberry.CustomAttributes;

namespace jmayberry.GeneralInfrastructure {
    public class GameManager : MonoBehaviour {
        [Header("Refrences")]
        [Required] public IPlayer player;

        [Required] CinemachineVirtualCamera vCam;
        [Readonly] CinemachineBasicMultiChannelPerlin vCam_noise;

        [Required] PostProcessVolume PPVolume;
        [Readonly] ChromaticAberration PPVolume_chromaticAberration;

        [Readonly] public LayerMask groundLayer;
        [Readonly] public LayerMask enemyLayer;

        [Header("Screen Shake")]
        [InspectorRename("Amplitude")] public float screenShake_amplitude = 7f;
        [InspectorRename("Frequency")] public float screenShake_frequency = 10f;
        [InspectorRename("Time")] public float screenShake_time = 0.08f;
        public UnityEvent EventScreenShakeStart;
        public UnityEvent EventScreenShakeEnd;

        [Header("Screen Pulse")]
        [InspectorRename("Intensity")] public float screenPulse_intensity = 0.2f;
        [InspectorRename("Time")] public float screenPulse_time = 0.08f;
        public UnityEvent EventScreenPulseStart;
        public UnityEvent EventScreenPulseEnd;

        [Header("Level Transitions")]
        [Required] public LevelTransition levelTransition;
        [InspectorRename("Change Scene on GameOver")] public bool gameOver_doReset;
        [InspectorRename("Game Over Scene")] public string gameOver_scene;

        [Header("Other")]
        [HideInInspector] public Scene currentScene;
        [Readonly] public bool is_paused;
        public UnityEvent EventPauseStart;
        public UnityEvent EventPauseEnd;
        public UnityEvent EventFreezeTimeStart;
        public UnityEvent EventFreezeTimeEnd;

        public UnityEvent EventGameOver;

        public static GameManager instance { get; private set; }
        private void Awake() {
            if (instance != null) {
                Debug.LogError("Found more than one GameManager in the scene.");
            }

            instance = this;

            this.currentScene = SceneManager.GetActiveScene();
            this.enemyLayer = LayerMask.GetMask("Enemy");
            this.groundLayer = LayerMask.GetMask("Ground");

            this.levelTransition = FindAnyObjectByType<LevelTransition>();

            this.vCam = FindAnyObjectByType<CinemachineVirtualCamera>();
            this.vCam_noise = this.vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

            this.PPVolume = FindAnyObjectByType<PostProcessVolume>();
            this.PPVolume.profile.TryGetSettings(out this.PPVolume_chromaticAberration);
        }

        public void GameOver() {
            EventGameOver.Invoke();

            if (!this.gameOver_doReset) {
                return;
            }

            if ((this.gameOver_scene != null) || (this.gameOver_scene != "")) {
                this.ChangeScene(this.gameOver_scene);
                return;
            }

            this.ChangeScene(this.currentScene.name);
        }

        public void ChangeScene(string sceneName) {
            levelTransition.ChangeScene(sceneName);
        }

        public bool CheckIsGround(Collider2D other) {
            // Convert the gameObject's layer into a bit mask
            int objectLayerMask = 1 << other.gameObject.layer;

            // Use bitwise AND to check if the masks overlap
            return ((this.groundLayer.value & objectLayerMask) > 0);
        }

        public IEnumerator ScreenShake(float? amplitude = null, float? frequency = null, float? time = null) {
            EventScreenShakeStart.Invoke();
            this.vCam_noise.m_AmplitudeGain = (amplitude.HasValue ? (float)amplitude : this.screenShake_amplitude);
            this.vCam_noise.m_FrequencyGain = (frequency.HasValue ? (float)frequency : this.screenShake_frequency);
            yield return new WaitForSeconds((time.HasValue ? (float)time : this.screenShake_time));
            this.vCam_noise.m_AmplitudeGain = 0;
            this.vCam_noise.m_FrequencyGain = 0;
            EventScreenShakeEnd.Invoke();
        }

        public IEnumerator ScreenPulse(float? intensity = null, float? time = null) {
            EventScreenPulseStart.Invoke();
            this.PPVolume_chromaticAberration.intensity.value = (intensity.HasValue ? (float)intensity : this.screenPulse_intensity);
            yield return new WaitForSeconds((time.HasValue ? (float)time : this.screenPulse_time));
            this.PPVolume_chromaticAberration.intensity.value = 0;
            EventScreenPulseEnd.Invoke();
        }

        public IEnumerator FreezeTime(float duration) {
            EventFreezeTimeStart.Invoke();
            Time.timeScale = 0;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1;
            EventFreezeTimeEnd.Invoke();
        }
    }
}