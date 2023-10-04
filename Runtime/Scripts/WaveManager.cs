using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using jmayberry.CustomAttributes;
using jmayberry.GeneralInfrastructure.Spawner;
using System;
using UnityEditor;
using UnityEngine.Splines;
using Unity.Mathematics;
using Cinemachine.Utility;

namespace jmayberry.GeneralInfrastructure.Manager {
	public interface IWave<T> where T : MonoBehaviour, ISpawnable {
		int count { get; set; }
		T[] possible { get; set; }
		float timeBetweenSpawns { get; set; }
	}

	public enum SpawnLocationType { WithinCircle, WithinBox, WithinCollider, FromList, AlongSpline, OnMesh }

	public abstract class WaveManagerBase<T, U> : MonoBehaviour
		where T : MonoBehaviour, ISpawnable
		where U : IWave<T>
	{
		[Header("Setup")]
		public U[] waves;
		public float timeBetweenWaves;
		public Transform spawnParent;

		[Header("Internal")]
		public UnitySpawner<T> spawner;
		public bool is2D = true;
		public SpawnLocationType spawnLocationType = SpawnLocationType.WithinCircle;
		public float spawnRadius;
		public Transform spawnRadiusCenter;
		public Bounds spawnBox;
		public Transform[] spawnPoints;
		public SplineContainer spawnSpline;
		public Collider2D spawnCollider;

		[Header("Debug")]
		[Readonly] public U currentWave;
		[Readonly] public int currentWaveIndex;

		[Header("GUI")]
		public UnityEvent<int> EventWaveStart;
		public UnityEvent<int> EventWaveEnd;
		public UnityEvent EventWavesOver;

		bool wavesFinished = false;

		void Start() {
			this.spawner = new UnitySpawner<T>();

			if (this.EventWaveStart == null) {
				this.EventWaveStart = new UnityEvent<int>();
			}

			if (this.EventWaveEnd == null) {
				this.EventWaveEnd = new UnityEvent<int>();
			}

			if (this.EventWavesOver == null) {
				this.EventWavesOver = new UnityEvent();
			}

			this.StartWaves();
		}

		void Update() {
			if (!wavesFinished) {
				return;
			}

			T[] spawnlingsInScene = FindObjectsByType<T>(FindObjectsSortMode.None);
			if (spawnlingsInScene.Length <= 0) {
				this.EventWavesOver.Invoke();
				this.wavesFinished = false;
				this.gameObject.SetActive(false);
			}
		}

		void StartWaves() {
			StartCoroutine(SpawnWaves());
		}

		IEnumerator SpawnWaves() {
			for (int i = 0; i < this.waves.Length; i++) {
				this.currentWaveIndex = i;

				this.EventWaveStart.Invoke(i);

				this.currentWave = waves[i];

				if (this.currentWave.count <= 0) {
					while (true) {
						if (this.wavesFinished) {
							yield break;
						}

						if (!this.DoSpawn(i, -1)) {
							break;
						}

						yield return new WaitForSeconds(this.currentWave.timeBetweenSpawns);
					}
				}
				else {
					for (int j = 0; j < this.currentWave.count; j++) {
						if (this.wavesFinished) {
							yield break;
						}

						if (!this.DoSpawn(i, j)) {
							break;
						}

						yield return new WaitForSeconds(this.currentWave.timeBetweenSpawns);
					}
				}

				this.EventWaveEnd.Invoke(i);

				yield return new WaitForSeconds(this.timeBetweenWaves);
			}

			this.wavesFinished = true;
		}

		private bool DoSpawn(int i, int j) {
			T randomPrefab = this.currentWave.possible[UnityEngine.Random.Range(0, this.currentWave.possible.Length)];
			Transform randomSpawnPoint = this.GetSpawnLocation();

			T spawnling = this.spawner.Spawn(randomPrefab, randomSpawnPoint, this.spawnParent);
			return this.OnSpawn(spawnling, this.currentWave, i, j);
		}

		public virtual Transform GetSpawnLocation() {
			Vector3 spawnPosition = Vector3.zero;
			switch (this.spawnLocationType) {
				case SpawnLocationType.FromList:
					if (this.spawnPoints.Length == 0) {
						throw new Exception("Missing *spawnPoints*");
					}

					return this.spawnPoints[UnityEngine.Random.Range(0, this.spawnPoints.Length)];

				case SpawnLocationType.WithinCircle: {
					if (this.spawnRadiusCenter == null) {
						throw new Exception("Missing *spawnRadiusCenter*");
					}

					if (this.spawnRadius == 0) {
						throw new Exception("Missing *spawnRadius*");
					}

					Vector3 randomDirection = UnityEngine.Random.insideUnitSphere;
					Vector3 scaledDirection = new Vector3(
						randomDirection.x * this.spawnRadius,
						randomDirection.y * this.spawnRadius,
						this.is2D ? 0 : randomDirection.z * this.spawnRadius
					);
					spawnPosition = spawnRadiusCenter.position + scaledDirection;
					break;
				}

				case SpawnLocationType.WithinBox: {
					if (this.spawnBox == null) {
						throw new Exception("Missing *spawnBox*");
					}

						spawnPosition = new Vector3(
						UnityEngine.Random.Range(this.spawnBox.min.x, this.spawnBox.max.x),
						UnityEngine.Random.Range(this.spawnBox.min.y, this.spawnBox.max.y),
						UnityEngine.Random.Range(this.spawnBox.min.z, this.spawnBox.max.z)
					);
					break;
				}

				case SpawnLocationType.AlongSpline: {
					if (this.spawnSpline == null) {
						throw new Exception("Missing *spawnSpline*");
					}

					spawnPosition = spawnSpline.EvaluatePosition(UnityEngine.Random.value);
					break;
				}

				case SpawnLocationType.WithinCollider: {
					if (this.spawnCollider == null) {
						throw new Exception("Missing *spawnCollider*");
					}

					if (this.is2D) {
						if (this.spawnCollider is BoxCollider2D box) {
							spawnPosition = new Vector3(
								UnityEngine.Random.Range(box.offset.x - box.size.x / 2, box.offset.x + box.size.x / 2),
								UnityEngine.Random.Range(box.offset.y - box.size.y / 2, box.offset.y + box.size.y / 2),
								0
							);
							break;
						}

						if (this.spawnCollider is CircleCollider2D circle) {
							Vector2 randomDirection = UnityEngine.Random.insideUnitCircle * circle.radius;
							spawnPosition = circle.offset + randomDirection;
							break;
						}

						if (this.spawnCollider is PolygonCollider2D) {
							for (int i = 0; i < 100; i++) {
								Vector2 randomPos = new Vector2(
									UnityEngine.Random.Range(this.spawnCollider.bounds.min.x, this.spawnCollider.bounds.max.x),
									UnityEngine.Random.Range(this.spawnCollider.bounds.min.y, this.spawnCollider.bounds.max.y)
								);

								if (this.spawnCollider.OverlapPoint(randomPos)) {
									spawnPosition = randomPos;
									break;
								}
							}

							if (spawnPosition == Vector3.zero) {
								Debug.LogWarning("Failed to find a valid spawn point within the polygon collider.");
							}
							
							break;
						}
					}

					throw new Exception("Unknown Collider Type");
				}

				case SpawnLocationType.OnMesh:
					// TODO: Use NavMesh.SamplePosition to find a valid spawn position on the NavMesh
					throw new Exception($"Unimplemented spawn location type '{this.spawnLocationType}'");

				default:
					throw new Exception($"Unknown spawn location type '{this.spawnLocationType}'");
			}

			Transform spawnTransform = new GameObject("SpawnPoint").transform;
			spawnTransform.position = spawnPosition;
			return spawnTransform;
		}

		public abstract bool OnSpawn(T spawnling, U wave, int waveIndex, int spawnlingIndex);

		public virtual void OnDrawGizmosSelected() {
			if (!this.enabled) {
				return;
			}

			Handles.color = Color.blue;
			switch (this.spawnLocationType) {
				case SpawnLocationType.FromList:
					foreach (var point in this.spawnPoints) {
						if (!this.is2D) {
							Handles.DrawWireDisc(point.position, Vector3.right, 0.5f);
							Handles.DrawWireDisc(point.position, Vector3.up, 0.5f);
						}
						Handles.DrawWireDisc(point.position, Vector3.forward, 0.5f);
					}
					break;

				case SpawnLocationType.WithinCircle:
					if ((this.spawnRadiusCenter == null) || (this.spawnRadius == 0)) {
						return;
					}

					if (!this.is2D) {
						Handles.DrawWireDisc(this.spawnRadiusCenter.position, Vector3.right, this.spawnRadius);
						Handles.DrawWireDisc(this.spawnRadiusCenter.position, Vector3.up, this.spawnRadius);
					}
					Handles.DrawWireDisc(this.spawnRadiusCenter.position, Vector3.forward, this.spawnRadius);
					break;

				case SpawnLocationType.WithinBox:
					if (this.spawnBox == null) {
						return;
					}

					Handles.DrawWireCube(this.spawnBox.center, this.spawnBox.size);
					break;
			}
		}
	}
}