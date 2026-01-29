using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MonsterSpawner))]
public class TestSpawner : MonoBehaviour
{
    [Header("Debug")]
    public bool spawnOnStart = true;
    public WaveStepDefinition testWave;

    [Header("Spawn Location")]
    public bool useTransformPosition = true;
    public Vector3 spawnCenterOffset;
    public float spawnRadius = 5f;

    private MonsterSpawner monsterSpawner;

    private void Awake()
    {
        monsterSpawner = GetComponent<MonsterSpawner>();
    }

    private void Start()
    {
        if (spawnOnStart)
        {
            StartCoroutine(SpawnWave(testWave));
        }
    }

    public IEnumerator SpawnWave(WaveStepDefinition wave)
    {
        if (wave == null)
        {
            yield break;
        }

        if (monsterSpawner == null)
        {
            monsterSpawner = GetComponent<MonsterSpawner>();
            if (monsterSpawner == null)
            {
                yield break;
            }
        }

        if (wave.entries == null)
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
                Vector3 spawnPos = GetRandomSpawnPosition();
                monsterSpawner.Spawn(entry.prefab, spawnPos, Quaternion.identity);
                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }
    }

    public Vector3 GetRandomSpawnPosition()
    {
        Vector3 spawnPos = spawnCenterOffset;
        if (useTransformPosition)
        {
            spawnPos += transform.position;
        }

        if (spawnRadius > 0f)
        {
            spawnPos += Random.insideUnitSphere * spawnRadius;
        }

        return spawnPos;
    }
}