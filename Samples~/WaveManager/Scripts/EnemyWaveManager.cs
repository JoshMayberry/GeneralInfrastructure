using UnityEngine;
using UnityEngine.Splines;
using jmayberry.GeneralInfrastructure.Manager;

[System.Serializable]
public class EnemyWave : IWave<Enemy> {
    public Enemy[] possible;
    public int count;
    public float timeBetweenSpawns;
    public SplineContainer enemyPath;

    int IWave<Enemy>.count { get => count; set => count = value; }
    Enemy[] IWave<Enemy>.possible { get => possible; set => possible = value; }
    float IWave<Enemy>.timeBetweenSpawns { get => timeBetweenSpawns; set => timeBetweenSpawns = value; }
}

public class EnemyWaveManager : WaveManagerBase<Enemy, EnemyWave> {
    public override void OnSpawn(Enemy spawnling, EnemyWave wave, int waveIndex, int spawnlingIndex) {
        spawnling.WalkPath(wave.enemyPath);
    }
}
