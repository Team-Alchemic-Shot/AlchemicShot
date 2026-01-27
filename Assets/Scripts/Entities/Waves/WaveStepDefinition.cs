using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Waves/Wave Step Definition", fileName = "WaveStepDefinition")]
public class WaveStepDefinition : ScriptableObject
{
    public SpawnEntry[] entries;
    public float spawnInterval = 0.5f;
}

[Serializable]
public struct SpawnEntry
{
    public GameObject prefab;
    public int count;
}
