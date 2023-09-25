using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using jmayberry.CustomAttributes;
using jmayberry.GeneralInfrastructure.Spawner;

namespace jmayberry.GeneralInfrastructure.Manager {
    public interface IWave<T> where T : MonoBehaviour, ISpawnable {
        int count { get; set; }
        T[] possible { get; set; }
        float timeBetweenSpawns { get; set; }
    }

    public abstract class WaveManagerBase<T, U> : MonoBehaviour
		where T : MonoBehaviour, ISpawnable
		where U : IWave<T>
	{
		[Header("Setup")]
		public U[] waves;
		public float timeBetweenWaves;

		[Header("Internal")]
		public UnitySpawner<T> spawner;
		public Transform[] spawnPoints;

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

			T[] enemiesInScene = FindObjectsByType<T>(FindObjectsSortMode.None);
			if (enemiesInScene.Length <= 0) {
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
				for (int j = 0; j < this.currentWave.count; j++) {
					if (this.wavesFinished) {
						yield break;
					}

					T randomPrefab = this.currentWave.possible[Random.Range(0, this.currentWave.possible.Length)];
					Transform randomSpawnPoint = this.spawnPoints[Random.Range(0, this.spawnPoints.Length)];

					T spawnling = this.spawner.Spawn(randomPrefab, randomSpawnPoint);
					this.OnSpawn(spawnling, this.currentWave, i, j);

					yield return new WaitForSeconds(this.currentWave.timeBetweenSpawns);
				}

				this.EventWaveEnd.Invoke(i);

				yield return new WaitForSeconds(this.timeBetweenWaves);
			}

			this.wavesFinished = true;
		}

		public abstract void OnSpawn(T spawnling, U wave, int waveIndex, int spawnlingIndex);
	}
}