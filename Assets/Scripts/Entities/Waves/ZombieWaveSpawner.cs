using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Manages zombie wave spawning with progression, difficulty scaling, and event callbacks.
/// Integrates with the existing wave system to handle multiple waves with configurable parameters.
/// </summary>
[RequireComponent(typeof(MonsterSpawner))]
public class ZombieWaveSpawner : MonoBehaviour
{
    [Header("Wave Configuration")]
    [SerializeField]
    private WaveStepDefinition[] waves;
    
    [SerializeField]
    private float delayBetweenWaves = 5f;

    [Header("Spawn Location")]
    [SerializeField]
    private bool useTransformPosition = true;
    
    [SerializeField]
    private Vector3 spawnCenterOffset = Vector3.zero;
    
    [SerializeField]
    private float spawnRadius = 10f;

    [Header("Difficulty Scaling")]
    [SerializeField]
    private bool enableDifficultyScaling = true;
    
    [SerializeField]
    [Range(0f, 2f)]
    private float difficultyScalePerWave = 0.1f;

    [Header("Auto Start")]
    [SerializeField]
    private bool autoStartWaves = true;

    [Header("Proximity Detection")]
    [SerializeField]
    private bool useProximityDetection = false;
    
    [SerializeField]
    private GameObject playerObject;
    
    [SerializeField]
    [Range(0.1f, 100f)]
    private float proximityRange = 30f;

    // Events
    public event Action<int> OnWaveStarted; // wave number (0-indexed)
    public event Action<int> OnWaveCompleted; // wave number (0-indexed)
    public event Action OnAllWavesCompleted;
    public event Action<string> OnStatusUpdate; // for UI feedback

    // State
    private MonsterSpawner monsterSpawner;
    private int currentWaveIndex = -1;
    private bool isSpawningWave = false;
    private bool hasStartedAnyWave = false;
    private bool playerInProximity = false;

    private void Awake()
    {
        monsterSpawner = GetComponent<MonsterSpawner>();
    }

    private void Start()
    {
        // Auto-find player if not assigned
        if (playerObject == null && useProximityDetection)
        {
            playerObject = GameObject.FindWithTag("Player");
        }

        // Only auto-start if not using proximity detection
        if (!useProximityDetection && autoStartWaves && waves != null && waves.Length > 0)
        {
            StartNextWave();
        }
    }

    private void Update()
    {
        // Check proximity detection
        if (useProximityDetection && playerObject != null && !playerInProximity)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerObject.transform.position);
            
            if (distanceToPlayer <= proximityRange)
            {
                playerInProximity = true;
                OnStatusUpdate?.Invoke($"Player entered range! Starting waves...");
                StartNextWave();
            }
        }
    }

    /// <summary>
    /// Starts the next wave in the sequence.
    /// </summary>
    public void StartNextWave()
    {
        if (isSpawningWave)
        {
            OnStatusUpdate?.Invoke("Wave already in progress");
            return;
        }

        if (waves == null || waves.Length == 0)
        {
            OnStatusUpdate?.Invoke("No waves configured");
            return;
        }

        currentWaveIndex++;

        if (currentWaveIndex >= waves.Length)
        {
            currentWaveIndex--;
            OnStatusUpdate?.Invoke("All waves completed!");
            OnAllWavesCompleted?.Invoke();
            return;
        }

        StartCoroutine(SpawnWaveCoroutine(currentWaveIndex));
    }

    /// <summary>
    /// Starts a specific wave by index.
    /// </summary>
    public void StartWave(int waveIndex)
    {
        if (isSpawningWave)
        {
            OnStatusUpdate?.Invoke("Wave already in progress");
            return;
        }

        if (waves == null || waves.Length == 0)
        {
            OnStatusUpdate?.Invoke("No waves configured");
            return;
        }

        if (waveIndex < 0 || waveIndex >= waves.Length)
        {
            OnStatusUpdate?.Invoke($"Invalid wave index: {waveIndex}");
            return;
        }

        currentWaveIndex = waveIndex;
        StartCoroutine(SpawnWaveCoroutine(waveIndex));
    }

    /// <summary>
    /// Restarts all waves from the beginning.
    /// </summary>
    public void RestartWaves()
    {
        StopAllCoroutines();
        currentWaveIndex = -1;
        isSpawningWave = false;
        hasStartedAnyWave = false;
        playerInProximity = false;
        OnStatusUpdate?.Invoke("Waves restarted");
        
        if (!useProximityDetection && autoStartWaves)
        {
            StartNextWave();
        }
    }

    /// <summary>
    /// Gets the total number of waves.
    /// </summary>
    public int GetTotalWaves() => waves?.Length ?? 0;

    /// <summary>
    /// Gets the current wave index (0-based).
    /// </summary>
    public int GetCurrentWaveIndex() => currentWaveIndex;

    /// <summary>
    /// Checks if currently spawning a wave.
    /// </summary>
    public bool IsSpawningWave() => isSpawningWave;

    private IEnumerator SpawnWaveCoroutine(int waveIndex)
    {
        isSpawningWave = true;
        hasStartedAnyWave = true;

        WaveStepDefinition wave = waves[waveIndex];

        OnWaveStarted?.Invoke(waveIndex);
        OnStatusUpdate?.Invoke($"Wave {waveIndex + 1}/{waves.Length} started");

        if (wave == null || wave.entries == null || wave.entries.Length == 0)
        {
            isSpawningWave = false;
            OnWaveCompleted?.Invoke(waveIndex);
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
                
                // Apply difficulty scaling if enabled
                GameObject spawnedPrefab = entry.prefab;
                if (enableDifficultyScaling && waveIndex > 0)
                {
                    spawnedPrefab = ApplyDifficultyScaling(entry.prefab, waveIndex);
                }

                monsterSpawner.Spawn(spawnedPrefab, spawnPos, Quaternion.identity);
                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }

        isSpawningWave = false;
        OnWaveCompleted?.Invoke(waveIndex);
        OnStatusUpdate?.Invoke($"Wave {waveIndex + 1} completed");

        // Wait before starting next wave
        yield return new WaitForSeconds(delayBetweenWaves);

        // Auto-start next wave
        StartNextWave();
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 spawnPos = spawnCenterOffset;
        
        if (useTransformPosition)
        {
            spawnPos += transform.position;
        }

        if (spawnRadius > 0f)
        {
            spawnPos += UnityEngine.Random.insideUnitSphere * spawnRadius;
        }

        return spawnPos;
    }

    /// <summary>
    /// Applies difficulty scaling to spawned entities.
    /// Can be extended to scale health, damage, speed, etc.
    /// </summary>
    private GameObject ApplyDifficultyScaling(GameObject prefab, int waveIndex)
    {
        // For now, just return the prefab as-is
        // You can extend this to:
        // - Increase health component values
        // - Increase damage output
        // - Increase movement speed
        // - Add visual indicators of difficulty
        
        return prefab;
    }
}
