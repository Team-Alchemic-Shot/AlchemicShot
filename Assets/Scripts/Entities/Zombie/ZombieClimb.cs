using UnityEngine;
using UnityEngine.AI;

public class ZombieClimb : MonoBehaviour
{
    [SerializeField]
    private Transform hitboxRoot;

    [SerializeField]
    private bool enabledByDefault = false;

    [SerializeField]
    private float chance = 0.15f;
    [SerializeField]
    private float height = 0.75f;
    [SerializeField]
    private float duration = 0.9f;
    [SerializeField]
    private float cooldown = 2.5f;

    [Header("Blocked Detection")]
    [SerializeField]
    private float minChaseTimeBeforeClimb = 0.75f; // don't immediately climb
    [SerializeField]
    private float blockedVelocityThreshold = 0.15f;
    [SerializeField]
    private float blockedCheckInterval = 0.25f;

    [Header("Stack Limit")]
    [SerializeField]
    private float nearbyClimberRadius = 1.25f;

    private NavMeshAgent agent;
    private ZombieTargeting targeting;

    private Vector3 hitboxBaseLocalPos;

    
  //apply the lift in world space, but still follow the zombie's movement.
    private Vector3 hitboxBaseWorldPos;

    private float climbTimer;
    private float cooldownTimer;

    private float chaseTimer;
    private float blockedTimer;

    private bool isLifted;

    private static readonly Collider[] nearby = new Collider[32];

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        targeting = GetComponent<ZombieTargeting>();

        if (hitboxRoot != null)
        {
            hitboxBaseLocalPos = hitboxRoot.localPosition;
            hitboxBaseWorldPos = hitboxRoot.position;
        }
    }

    public void ApplyArchetype(ZombieArchetype data)
    {
        if (data == null)
        {
            return;
        }

        enabledByDefault = data.canclimb;
        chance = data.climbChance;
        height = data.climbHeight;
        duration = data.climbDuration;
        cooldown = data.climbCooldown;
    }

    private void Update()
    {
        if (!enabledByDefault)
        {
            return;
        }

        if (hitboxRoot == null)
        {
            return;
        }

        // preserve position
        if (!isLifted)
        {
            hitboxBaseLocalPos = hitboxRoot.localPosition;
            hitboxBaseWorldPos = hitboxRoot.position;
        }

        if (isLifted)
        {
            
            hitboxRoot.position = GetBaseWorldPosNoScale() + Vector3.up * height;

            climbTimer -= Time.deltaTime;
            if (climbTimer <= 0f)
            {
                EndClimb();
            }

            return;
        }

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        //reset chaseTimer so we require a fresh chase window next time.
        if (targeting == null || targeting.CurrentTarget == null) 
        {
            chaseTimer = 0f;
            return;
        }

        // climb delay
        chaseTimer += Time.deltaTime;
        if (chaseTimer < minChaseTimeBeforeClimb)
        {
            return;
        }

        blockedTimer -= Time.deltaTime;
        if (blockedTimer > 0f)
        {
            return;
        }

        blockedTimer = blockedCheckInterval;

        if (agent == null || !agent.isOnNavMesh)
        {
            return;
        }

        
        if (agent.velocity.magnitude > blockedVelocityThreshold)
        {
            return;
        }

        // limit stack for now
        if (HasLiftedClimberNearby())
        {
            return;
        }

        if (Random.value > chance)
        {
            return;
        }

        BeginClimb();
    }

    private bool HasLiftedClimberNearby()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, nearbyClimberRadius, nearby);
        for (int i = 0; i < count; i++)
        {
            var c = nearby[i];
            if (c == null)
            {
                continue;
            }

            var other = c.GetComponentInParent<ZombieClimb>();
            if (other == null || other == this)
            {
                continue;
            }

            if (other.isLifted)
            {
                return true;
            }
        }

        return false;
    }

    private void BeginClimb()
    {
        isLifted = true;
        climbTimer = duration;
        cooldownTimer = cooldown;

        
        hitboxRoot.position = GetBaseWorldPosNoScale() + Vector3.up * height;
    }

    private void EndClimb()
    {
        isLifted = false;

        // restore local pose 
        hitboxRoot.localPosition = hitboxBaseLocalPos;
        hitboxBaseWorldPos = hitboxRoot.position;
    }

    private Vector3 GetBaseWorldPosNoScale()
    {
        if (hitboxRoot == null)
        {
            return transform.position;
        }

        var p = hitboxRoot.parent;
        if (p == null)
        {
            return hitboxBaseWorldPos;
        }

        
        return p.position + (p.rotation * hitboxBaseLocalPos);
    }
}