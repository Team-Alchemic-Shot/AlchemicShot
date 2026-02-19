using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Resets the entity's position if it falls below a minimum Y level.
/// </summary>
public class MinYRespawn : EntityComponent
{
    [Tooltip("The Y level below which the entity will be respawned.")]
    [SerializeField]
    private float minimumY = -20f;

    [Tooltip("Radius around the spawn point to randomly position the entity upon respawn.")]
    [SerializeField]
    private float spawnRadius = 2f;

    private Vector3 initialPosition;
    private Transform spawnPoint;

    protected void Start()
    {
        initialPosition = transform.position;
    }

    private void Update()
    {
        if (transform.position.y < minimumY)
        {
            Respawn();
        }
    }

    public void SetRespawnPoint(Transform newSpawnPoint)
    {
        spawnPoint = newSpawnPoint;
    }

    private void Respawn()
    {
        Vector3 center = spawnPoint != null ? spawnPoint.position : initialPosition;
        
        // Generate a random position within the radius on the XZ plane
        Vector2 randomPoint = Random.insideUnitCircle * spawnRadius;
        Vector3 newPosition = new(center.x + randomPoint.x, center.y, center.z + randomPoint.y);

        // Reset velocity if a Rigidbody is present
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Handle NavMeshAgent if present
        if (TryGetComponent<NavMeshAgent>(out var agent))
        {
            agent.Warp(newPosition);
        }
        else
        {
            // Move the entity directly
            transform.position = newPosition;
        }
    }
}
