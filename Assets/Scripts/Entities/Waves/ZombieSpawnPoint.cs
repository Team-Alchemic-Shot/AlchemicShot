using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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
    [SerializeField]
    private float navMeshSnapDistance = 5f; 
    
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
    private Coroutine spawnCoroutine;
    private readonly Collider[] overlapBuffer = new Collider[12];

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

        while (isActiveForCurrentRound && roundManager.TryRegisterSpawn())
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            GameObject zombieInstance = monsterSpawner.Spawn(zombiePrefab, spawnPos, Quaternion.identity);
            // var comp = zombieInstance.AddComponent<MinYRespawn>();
            // comp.SetRespawnPoint(transform);

            ResolveSpawnOverlap(zombieInstance);
            PlaceOnNavMesh(zombieInstance);

            // Apply difficulty scaling
            ApplyDifficultyToZombie(zombieInstance);

            yield return new WaitForSeconds(config.spawnInterval);
            
        }
        spawnCoroutine = null;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 spawnPos = transform.position + spawnCenterOffset;

        if (spawnRadius > 0f)
        {
            // XZ only (do NOT randomize Y)
            Vector2 offset2 = Random.insideUnitCircle * spawnRadius;
            spawnPos += new Vector3(offset2.x, 0f, offset2.y);
        }

        // try to snap the chosen point onto the NavMesh before spawning.
        if (NavMesh.SamplePosition(spawnPos, out var hit, navMeshSnapDistance, NavMesh.AllAreas))
        {
            spawnPos = hit.position;
        }

        return spawnPos;
    }

    private void ApplyDifficultyToZombie(GameObject zombieInstance)
    {
        if (zombieInstance == null)
            return;

        float difficultyMultiplier = roundManager.GetDifficultyMultiplier();

        // Apply scaling to health component if it exists
        if (zombieInstance.TryGetComponent<Health>(out var health))
        {
            health.ScaleMaxHealth(difficultyMultiplier);
        }

        // Apply scaling to damage/attack component if needed DISABLED
        if (zombieInstance.TryGetComponent<ZombieAttack>(out var zombieAttack))
        {
            // zombieAttack.ScaleDamage(difficultyMultiplier);
        }
    }

    private void ResolveSpawnOverlap(GameObject zombieInstance)
    {
        if (zombieInstance == null)
        {
            return;
        }

        var zombieCollider = zombieInstance.GetComponentInChildren<Collider>();
        if (zombieCollider == null)
        {
            return;
        }

        const int maxIterations = 6;
        const float pushPadding = 0.02f;

        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            var bounds = zombieCollider.bounds;
            int hitCount = Physics.OverlapBoxNonAlloc(
                bounds.center,
                bounds.extents,
                overlapBuffer,
                zombieCollider.transform.rotation,
                ~0,
                QueryTriggerInteraction.Ignore);

            bool moved = false;
            for (int i = 0; i < hitCount; i++)
            {
                var hit = overlapBuffer[i];
                if (hit == null || hit == zombieCollider)
                {
                    continue;
                }

                if (hit.transform.IsChildOf(zombieInstance.transform))
                {
                    continue;
                }

                if (Physics.ComputePenetration(
                        zombieCollider,
                        zombieCollider.transform.position,
                        zombieCollider.transform.rotation,
                        hit,
                        hit.transform.position,
                        hit.transform.rotation,
                        out Vector3 direction,
                        out float distance))
                {
                    zombieInstance.transform.position += direction * (distance + pushPadding);
                    Debug.Log($"Resolved spawn overlap for {zombieInstance.name} by moving {direction * (distance + pushPadding)}");
                    moved = true;
                }
            }

            if (!moved)
            {
                break;
            }
        }
    }


    private void PlaceOnNavMesh(GameObject zombieInstance)
    {
        if (zombieInstance == null)
        {
            return;
        }

        if (!zombieInstance.TryGetComponent<NavMeshAgent>(out var agent))
        {
            return;
        }

        if (agent.isOnNavMesh)
        {
            return;
        }

        // snap to nearest point on the navmesh
        if (NavMesh.SamplePosition(zombieInstance.transform.position, out var hit, navMeshSnapDistance, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            return;
        }

        Debug.LogWarning($"ZombieSpawnPoint {spawnPointId}: Spawned zombie off NavMesh and could not find NavMesh within {navMeshSnapDistance} units.", zombieInstance);
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
