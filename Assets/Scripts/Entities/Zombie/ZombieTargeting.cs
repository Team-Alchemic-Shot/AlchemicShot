using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(NavMeshAgent))]
public class ZombieTargeting : MonoBehaviour
{
    [SerializeField]
    private float detectionRadius = 10f;
    [SerializeField]
    private LayerMask targetMask = ~0;
    [SerializeField]
    private float refreshInterval = 0.5f;
    [SerializeField]
    private int maxTargetColliders = 16;

    public Transform CurrentTarget { get; private set; }

    private float refreshTimer;

    private NavMeshAgent agent;
    private Collider[] targetBuffer; // TODO realloc on WaveManager round change based on max monsters

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        targetBuffer = new Collider[Mathf.Max(1, maxTargetColliders)];
    }

    private void Update()
    {
        refreshTimer -= Time.deltaTime;
        if (refreshTimer <= 0f)
        {
            refreshTimer = refreshInterval;
            AcquireTarget();
        }

        if (agent != null && CurrentTarget != null)
        {
            agent.SetDestination(CurrentTarget.position);
        }
    }

    private void AcquireTarget()
    {
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
            if (hit == null)
            {
                continue;
            }

            if (hit.attachedRigidbody != null && hit.attachedRigidbody.gameObject == gameObject)
            {
                continue;
            }

            if (hit.GetComponentInParent<IDamageable>() == null)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestTarget = hit.transform;
            }
        }

        CurrentTarget = bestTarget;
    }
}
