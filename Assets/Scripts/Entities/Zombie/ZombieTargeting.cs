using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(NavMeshAgent))]

public class ZombieTargeting : MonoBehaviour
{
    [SerializeField]
    private float detectionRadius = 20f;
    [SerializeField]
    private LayerMask targetMask = ~0;
    [SerializeField]
    private float refreshInterval = 0.5f;
    [SerializeField]
    private int maxTargetColliders = 16;

    [SerializeField]
    private float baseSpeed = 2f;
    [SerializeField]
    private float chaseSpeed = 6f;
    [SerializeField]
    private float wanderRadius = 50f;
    [SerializeField]
    private float wanderInterval = 2f;

    [SerializeField]
    private Animator animator;

    private int walkHash;
    private int runHash;


    public Transform CurrentTarget { get; private set; }

    float refreshTimer; private NavMeshAgent agent;

    private Collider[] targetBuffer; // TODO realloc on WaveManager round change based on max monsters
    private float wanderTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        targetBuffer = new Collider[Mathf.Max(1, maxTargetColliders)];

        walkHash = Animator.StringToHash("IsWalking");
        runHash = Animator.StringToHash("IsRunning");
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
            //chase
            agent.speed = chaseSpeed;
            agent.SetDestination(CurrentTarget.position);

            if (animator.GetBool(runHash) == false)
            {
                animator.SetBool(walkHash, false);
                animator.SetBool(runHash, true);
            }
        }
        else
        {
            //wonder
            agent.speed = baseSpeed;
            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0f)
            {
                wanderTimer = wanderInterval; SetRandomWanderDestination();
            }
            if (animator.GetBool(walkHash) == false)
            {
                animator.SetBool(walkHash, true);
                animator.SetBool(runHash, false);
            }
        }

    }
    private void AcquireTarget()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, targetBuffer, targetMask);
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
                bestDistance = distance; bestTarget = hit.transform;
            }
        }
        CurrentTarget = bestTarget;
    }

    private void SetRandomWanderDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(navHit.position);
        }
    }
}