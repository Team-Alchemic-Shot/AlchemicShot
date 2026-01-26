using System;
using System.Collections;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public event Action<GameObject> OnMonsterSpawned;

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject instance = Instantiate(prefab, position, rotation);
        OnMonsterSpawned?.Invoke(instance);
        return instance;
    }

    public IEnumerator SpawnWave(WaveDefinition wave)
    {
        if (wave == null)
        {
            yield break;
        }

        foreach (var entry in wave.entries)
        {
            if (entry.prefab == null || entry.count <= 0)
            {
                continue;
            }

            for (int i = 0; i < entry.count; i++)
            {
                Vector3 spawnPos = wave.spawnCenter + UnityEngine.Random.insideUnitSphere * wave.spawnRadius;
                spawnPos.y = wave.spawnCenter.y;
                Spawn(entry.prefab, spawnPos, Quaternion.identity);
                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }
    }
}

[System.Serializable]
public class WaveDefinition
{
    public SpawnEntry[] entries;
    public float spawnInterval = 0.5f;
    public Vector3 spawnCenter;
    public float spawnRadius = 5f;
}

[System.Serializable]
public struct SpawnEntry
{
    public GameObject prefab;
    public int count;
}
