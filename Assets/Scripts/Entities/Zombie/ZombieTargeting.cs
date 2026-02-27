using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(NavMeshAgent))]
public class ZombieTargeting : MonoBehaviour
{
    
    [SerializeField, HideInInspector]
    private float detectionRadius = 20f;
    [SerializeField]
    private LayerMask targetMask = ~0;
    [SerializeField, HideInInspector]
    private float refreshInterval = 0.5f;
    [SerializeField, HideInInspector]
    private int maxTargetColliders = 16;

    [SerializeField, HideInInspector]
    private float baseSpeed = 2f;
    [SerializeField, HideInInspector]
    private float chaseSpeed = 6f;

    [SerializeField, HideInInspector]
    private float wanderRadius = 50f;
    [SerializeField, HideInInspector]
    private float wanderInterval = 2f;

    
    [SerializeField, HideInInspector]
    private float repathInterval = 0.25f;  //update path
    [SerializeField, HideInInspector]
    private float orbitRadius = 1.5f; // surround player
    [SerializeField, HideInInspector]
    private float orbitAngularSpeed = 0.8f;
    [SerializeField, HideInInspector]
    private float chaseStoppingDistance = 1.25f; // how close to player before stopping

    public Transform CurrentTarget { get; private set; }

    // ZombieSpeedController reads this AI should not directly set agent.speed.
    public float DesiredSpeed { get; private set; }

    private float refreshTimer;
    private float wanderTimer;
    private float repathTimer;

    private NavMeshAgent agent;
    private Collider[] targetBuffer;

    private float orbitSeed;

    public event Action walking;
    public event Action running;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        targetBuffer = new Collider[Mathf.Max(1, maxTargetColliders)];

        orbitSeed = (GetInstanceID() % 10000) * 0.001f;
    }

    private void Update()
    {
        refreshTimer -= Time.deltaTime;
        if (refreshTimer <= 0f)
        {
            refreshTimer = refreshInterval;
            AcquireTarget();
        }

        if (agent == null || !agent.isOnNavMesh)
        {
            DesiredSpeed = 0f;
            return;
        }

        if (CurrentTarget != null)
        {
            // chase
            DesiredSpeed = chaseSpeed;

            if (agent.stoppingDistance != chaseStoppingDistance)
            {
                agent.stoppingDistance = chaseStoppingDistance;
            }

            repathTimer -= Time.deltaTime;
            if (repathTimer <= 0f)
            {
                repathTimer = repathInterval;

                var targetPos = CurrentTarget.position;

                // spread zombies around the player
                if (orbitRadius > 0.01f)
                {
                    var a = orbitSeed + (Time.time * orbitAngularSpeed);
                    var offset = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * orbitRadius;
                    targetPos += offset;
                }

                agent.SetDestination(targetPos);
            }

            running?.Invoke();

            return;
        }

        // wander
        DesiredSpeed = baseSpeed;

        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f)
        {
            wanderTimer = wanderInterval;
            SetRandomWanderDestination();
        }

        walking?.Invoke();
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
                bestDistance = distance;
                bestTarget = hit.transform;
            }
        }

        CurrentTarget = bestTarget;
    }

    private void SetRandomWanderDestination()
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, wanderRadius, NavMesh.AllAreas) && agent.isOnNavMesh)
        {
            agent.SetDestination(navHit.position);
        }
    }

    public void ApplyArchetype(ZombieArchetype data)
    {
        if (data == null)
        {
            return;
        }
        Debug.Log($"[ZombieTargeting] Applied archetype {data.name} to {name}", gameObject);

        detectionRadius = data.detectionRadius;
        refreshInterval = data.refreshInterval;
        maxTargetColliders = data.maxTargetColliders;
        targetBuffer = new Collider[Mathf.Max(1, maxTargetColliders)];

        baseSpeed = data.baseSpeed;
        chaseSpeed = data.chaseSpeed;

        wanderRadius = data.wanderRadius;
        wanderInterval = data.wanderInterval;

        repathInterval = data.repathInterval;
        orbitRadius = data.orbitRadius;
        orbitAngularSpeed = data.orbitAngularSpeed;
        chaseStoppingDistance = data.chaseStoppingDistance;
    }

    void OnDisable()
    {
        Debug.Log($"[Zombie] DISABLED {name} at {transform.position}\n{Environment.StackTrace}", this);
    }

    void OnDestroy()
    {
        Debug.Log($"[Zombie] DESTROYED {name} at {transform.position}\n{Environment.StackTrace}", this);
    }
}