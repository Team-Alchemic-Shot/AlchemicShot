using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(NavMeshAgent))]
public class ZombieTargeting : MonoBehaviour
{
    [SerializeField]
    private float detectionRadius = 30f;
    [SerializeField]
    private LayerMask targetMask = ~0;
    [SerializeField]
    private float refreshInterval = 2f;
    [SerializeField]
    private int maxTargetColliders = 16;

    [SerializeField]
    private float baseSpeed = 2f;
    [SerializeField]
    private float chaseSpeed = 5f;
    [SerializeField]
    private float wanderRadius = 50f;
    [SerializeField]
    private float wanderInterval = 2f;

    public Transform CurrentTarget { get; private set; }

    private float refreshTimer;

    private NavMeshAgent agent;
    private Collider[] targetBuffer; // TODO realloc on WaveManager round change based on max monsters

    private float wanderTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        targetBuffer = new Collider[Mathf.Max(1, maxTargetColliders)];
    }

    private void Update()
    {
        // Only search if we don't already have a target
        if (CurrentTarget == null)
        {
            refreshTimer -= Time.deltaTime;
            if (refreshTimer <= 0f)
            {
                refreshTimer = refreshInterval;
                AcquireTarget();
            }
        }

        if (CurrentTarget != null)
        {
            float dist = Vector3.Distance(transform.position, CurrentTarget.position);

            // Lose target if too far
            if (dist > detectionRadius * 1.5f)
            {
                CurrentTarget = null;
                return;
            }

            agent.isStopped = false;
            agent.speed = chaseSpeed;
            agent.SetDestination(CurrentTarget.position);
        }
        else
        {
            agent.speed = baseSpeed;
            agent.isStopped = false;

            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0f)
            {
                wanderTimer = wanderInterval;
                SetRandomWanderDestination();
            }
        }
    }

    private void AcquireTarget()
    {
        // If we already have a target, keep it unless it's too far
        if (CurrentTarget != null)
        {
            float dist = Vector3.Distance(transform.position, CurrentTarget.position);

            if (dist <= detectionRadius * 1.2f) // small buffer so it doesn't flicker
            {
                return;
            }
            else
            {
                CurrentTarget = null;
            }
        }

        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            detectionRadius,
            targetBuffer,
            targetMask);

        float bestDistance = float.MaxValue;
        Transform bestTarget = null;

        for (int i = 0; i < hitCount; i++)
        {
            var hit = targetBuffer[i];
            if (hit == null) continue;

            if (hit.attachedRigidbody != null &&
                hit.attachedRigidbody.gameObject == gameObject)
                continue;

            var damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable == null) continue;

            float distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestTarget = hit.transform;
            }
        }

        CurrentTarget = bestTarget;
    }

    private void SetRandomWanderDestination()
    {
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        Vector3 randomDirection = new Vector3(randomCircle.x, 0f, randomCircle.y);
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(navHit.position);
        }
    }
}
