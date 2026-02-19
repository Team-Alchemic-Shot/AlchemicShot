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

    [Tooltip("Optional transform to use as the spawn center. If null, uses the initial starting position.")]
    [SerializeField]
    private Transform spawnPoint;

    [Tooltip("Radius around the spawn point to randomly position the entity upon respawn.")]
    [SerializeField]
    private float spawnRadius = 2f;

    private Vector3 initialPosition;

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

    private void Respawn()
    {
        Vector3 center = spawnPoint != null ? spawnPoint.position : initialPosition;
        
        // Generate a random position within the radius on the XZ plane
        Vector2 randomPoint = Random.insideUnitCircle * spawnRadius;
        Vector3 newPosition = new Vector3(center.x + randomPoint.x, center.y, center.z + randomPoint.y);

        // Reset velocity if a Rigidbody is present
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Handle NavMeshAgent if present
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
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
