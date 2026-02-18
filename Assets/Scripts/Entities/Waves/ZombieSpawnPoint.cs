using System.Collections;
using UnityEngine;

/// <summary>
/// Individual zombie spawn point with proximity detection.
/// Spawns zombies when player is near and round is active.
/// Integrates with ZombieRoundManager for round coordination.
/// </summary>
[RequireComponent(typeof(MonsterSpawner))]
public class ZombieSpawnPoint : MonoBehaviour
{
    [Header("Spawn Configuration")]
    [SerializeField]
    private int spawnPointId = 0;

    [SerializeField]
    [Tooltip("The map location ID this spawn point belongs to (0 is always unlocked)")]
    private int locationId = 0;
    
    [SerializeField]
    private float proximityRange = 40f;
    
    [SerializeField]
    private GameObject zombiePrefab;
    
    [SerializeField]
    private float spawnRadius = 5f;
    
    [Tooltip("Offset from this transform for spawn position")]
    [SerializeField]
    private Vector3 spawnCenterOffset = Vector3.zero;

    [Header("Debug")]
    [SerializeField]
    private bool visualizeProximityRange = true;

    // State
    private MonsterSpawner monsterSpawner;
    private ZombieRoundManager roundManager;
    private GameObject player;
    private bool playerInProximity = false;
    private bool isActiveForCurrentRound = false;
    private int spawnedInCurrentRound = 0;
    private Coroutine spawnCoroutine;

    private void Awake()
    {
        monsterSpawner = GetComponent<MonsterSpawner>();
    }

    private void Start()
    {
        // Find systems
        roundManager = ZombieRoundManager.Instance;
        player = GameObject.FindWithTag("Player");

        if (roundManager != null)
        {
            roundManager.RegisterSpawnPoint(this);
        }

        if (zombiePrefab == null)
        {
            Debug.LogWarning($"ZombieSpawnPoint {spawnPointId}: No zombie prefab assigned!", gameObject);
        }
    }

    private void Update()
    {
        if (roundManager == null || player == null)
            return;

        // Check proximity
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        bool wasInProximity = playerInProximity;
        playerInProximity = distanceToPlayer <= proximityRange;

        // When player enters range, activate spawning
        if (playerInProximity && !wasInProximity && isActiveForCurrentRound)
        {
            StartSpawning();
        }

        // When player leaves range, can optionally stop spawning
        if (!playerInProximity && wasInProximity)
        {
            // Don't stop - zombies already spawned will still attack
        }
    }

    /// <summary>
    /// Called by ZombieRoundManager when a new round starts.
    /// </summary>
    public void ActivateForRound(int roundNumber)
    {
        isActiveForCurrentRound = true;
        spawnedInCurrentRound = 0;
        playerInProximity = false;

        // Start spawning if player is already in range
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer <= proximityRange)
        {
            StartSpawning();
        }
    }

    /// <summary>
    /// Gets the map location ID this spawn point is associated with.
    /// </summary>
    public int GetLocationId() => locationId;

    /// <summary>
    /// Deactivates this spawn point (e.g., when map area is locked).
    /// </summary>
    public void Deactivate()
    {
        isActiveForCurrentRound = false;
        StopSpawning();
    }

    private void StartSpawning()
    {
        if (spawnCoroutine != null)
            return;

        spawnCoroutine = StartCoroutine(SpawnZombiesCoroutine());
    }

    private void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    private IEnumerator SpawnZombiesCoroutine()
    {
        ZombieRoundManager.RoundConfig config = roundManager.GetCurrentRoundConfig();
        
        // Calculate how many zombies this spawn point should spawn
        int zombiesToSpawn = config.zombiesPerSpawnPoint;

        for (int i = 0; i < zombiesToSpawn; i++)
        {
            if (!isActiveForCurrentRound)
                break;

            Vector3 spawnPos = GetRandomSpawnPosition();
            GameObject zombieInstance = monsterSpawner.Spawn(zombiePrefab, spawnPos, Quaternion.identity);

            // Apply difficulty scaling
            ApplyDifficultyToZombie(zombieInstance);

            // Notify round manager
            roundManager.OnZombieSpawned();
            spawnedInCurrentRound++;

            yield return new WaitForSeconds(config.spawnInterval);
        }

        spawnCoroutine = null;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 spawnPos = transform.position + spawnCenterOffset;

        if (spawnRadius > 0f)
        {
            spawnPos += UnityEngine.Random.insideUnitSphere * spawnRadius;
        }

        return spawnPos;
    }

    private void ApplyDifficultyToZombie(GameObject zombieInstance)
    {
        if (zombieInstance == null)
            return;

        float difficultyMultiplier = roundManager.GetDifficultyMultiplier();

        // Apply scaling to health component if it exists
        Health health = zombieInstance.GetComponent<Health>();
        if (health != null)
        {
            // Example: scaling max health
            // You'd need to implement this in your Health class
            // health.ScaleMaxHealth(difficultyMultiplier);
        }

        // Apply scaling to damage/attack component if needed
        ZombieAttack zombieAttack = zombieInstance.GetComponent<ZombieAttack>();
        if (zombieAttack != null)
        {
            // zombieAttack.ScaleDamage(difficultyMultiplier);
        }
    }

    /// <summary>
    /// Gets the ID of this spawn point.
    /// </summary>
    public int GetSpawnPointId() => spawnPointId;

    /// <summary>
    /// Gets proximity range for this spawn point.
    /// </summary>
    public float GetProximityRange() => proximityRange;

    private void OnDrawGizmos()
    {
        if (!visualizeProximityRange)
            return;

        // Draw proximity range circle
        Gizmos.color = isActiveForCurrentRound ? Color.green : Color.red;
        DrawCircle(transform.position, proximityRange, 32);

        // Draw spawn radius
        Gizmos.color = Color.yellow;
        DrawCircle(transform.position + spawnCenterOffset, spawnRadius, 16);
    }

    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angle = 0f;
        float angleStep = 360f / segments;
        Vector3 lastPoint = center + new Vector3(radius, 0, 0);

        for (int i = 0; i < segments; i++)
        {
            angle += angleStep;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(rad) * radius, 0, Mathf.Sin(rad) * radius);
            Gizmos.DrawLine(lastPoint, newPoint);
            lastPoint = newPoint;
        }
    }
}
