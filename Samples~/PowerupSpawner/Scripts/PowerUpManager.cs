using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using jmayberry.GeneralInfrastructure.Manager;

public class PowerUpWaveManager : WaveManagerBase<PowerUp, PowerUpWave> {
    public override bool OnSpawn(PowerUp spawnling, PowerUpWave wave, int waveIndex, int spawnlingIndex) {
        return true;
    }
}

[System.Serializable]
public class PowerUpWave : IWave<PowerUp> {
    public PowerUp[] possible;
    public int count;
    public float timeBetweenSpawns;

    int IWave<PowerUp>.count { get => count; set => count = value; }
    PowerUp[] IWave<PowerUp>.possible { get => possible; set => possible = value; }
    float IWave<PowerUp>.timeBetweenSpawns { get => timeBetweenSpawns; set => timeBetweenSpawns = value; }
}
