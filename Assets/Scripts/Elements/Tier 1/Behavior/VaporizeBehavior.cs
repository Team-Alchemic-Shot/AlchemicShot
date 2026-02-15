using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "VaporizeBehavior", menuName = "Elements/Behaviors/Tier 1/Vaporize")]
public class VaporizeBehavior : ElementBehavior
{
    [Header("Vaporize Settings")]
    [SerializeField]
    private float tickInterval = 0.5f;
    [SerializeField]
    private bool refreshDuration = true;
    [SerializeField]
    private bool stackIntensity = false;
    [SerializeField]
    private bool logTicks = true;
    [SerializeField]
    private LayerMask floorMask = 1;
    [SerializeField]
    private LayerMask enemyMask = 1;
    [SerializeField]
    private float range = 5f;
    [SerializeField]
    private float lateralImpulse = 1.5f;

    [Header("Fire DOT Settings")]
    [SerializeField]
    private float fireTickInterval = 0.5f;
    [SerializeField]
    private bool fireRefreshDuration = true;
    [SerializeField]
    private bool fireStackIntensity = false;
    [SerializeField]
    private bool fireLogTicks = true;
    [SerializeField]
    private float fireDefaultIntensity = 15f;
    [SerializeField]
    private float fireDuration = 3f;

    private float originalFireIntensity;
    private NavMeshAgent agentRef;
    private ZombieTargeting targetingRef;

    public override void Apply(ElementBehaviorContext context)
    {
        if (context.target == null)
        {
            return;
        }

        // find nearby enemies (including the shot target)
        var center = context.target.transform.position;
        var hits = Physics.OverlapSphere(center, range, enemyMask); // TODO buffer?
        if (hits == null || hits.Length == 0)
        {
            // fallback to just the hit target
            ApplyToTarget(context);
            return;
        }

        foreach (var hit in hits)
        {
            if (hit == null)
            {
                continue;
            }

            // resolve target from collider
            var target = hit.attachedRigidbody != null ? hit.attachedRigidbody.gameObject : hit.gameObject;
            var targetContext = new ElementBehaviorContext
            {
                instigator = context.instigator,
                target = target,
                position = target.transform.position,
                sourceBullet = context.sourceBullet,
            };
            if (target == context.target)
            {
                // primary target uses the existing behavior instance
                ApplyToTarget(targetContext);
                continue;
            }

            // ensure aoe targets have their own behavior instance + status tracking
            if (!target.TryGetComponent<ElementStatus>(out var elementStatus))
            {
                elementStatus = target.AddComponent<ElementStatus>();
            }
            elementStatus.AddElement(targetContext.sourceBullet.element);

            var behaviorInstance = Instantiate(this);
            behaviorInstance.SetOwnerElement(targetContext.sourceBullet.element);
            behaviorInstance.MarkRuntimeInstance();
            elementStatus.RegisterBehaviorInstance(targetContext.sourceBullet.element, behaviorInstance);
            behaviorInstance.ApplyToTarget(targetContext);
        }
    }

    private void ApplyToTarget(ElementBehaviorContext context)
    {
        // get agent 
        if (!context.target.TryGetComponent(out agentRef))
        {
            return;
        }
        agentRef.enabled = false; // needs to be disabled to leave the navmesh

        // get targeting
        if (!context.target.TryGetComponent(out targetingRef))
        {
            return;
        }
        targetingRef.enabled = false; // disable targeting becuase now the agent is disabled

        // throw enemy into the air
        if (!context.target.TryGetComponent<Rigidbody>(out var rigidbody))
        {
            return;
        }
        var lateral = Random.insideUnitSphere;
        lateral.y = 0f;
        if (lateral.sqrMagnitude > 0.0001f)
        {
            lateral.Normalize();
        }
        rigidbody.AddForce(Vector3.up * defaultIntensity + lateral * lateralImpulse, ForceMode.Impulse); // default intensity is used as the jump strength

        // piggyback on fire tag with recontextualization
        if (!context.target.TryGetComponent<FireBehavior.FireDOTTag>(out var fireTag))
        {
            fireTag = context.target.AddComponent<FireBehavior.FireDOTTag>();
        }
        // no tracking fire tag

        originalFireIntensity = fireTag.Intensity; // save for reversion

        fireTag.Apply( // reapply fire tag to recontextualize it with the plasma's context
            fireDuration, // will expire and auto clean up
            fireTickInterval,
            fireDefaultIntensity,
            fireRefreshDuration,
            fireStackIntensity,
            context.instigator,
            fireLogTicks);

        if (!context.target.TryGetComponent<VaporizeTag>(out var vaporizeTag))
        {
            vaporizeTag = context.target.AddComponent<VaporizeTag>();
        }
        TrackTag(vaporizeTag, context);

        vaporizeTag.SetContext(floorMask);
        vaporizeTag.Apply(
            duration,
            tickInterval,
            defaultIntensity,
            refreshDuration,
            stackIntensity,
            context.instigator,
            logTicks);
    }

    public override void RevertEffects(ElementBehaviorContext context)
    {
        if (context.target == null)
        {
            return;
        }

        // revert fire tag if it exists
        if (context.target.TryGetComponent<FireBehavior.FireDOTTag>(out var fireTag))
        {
            fireTag.Intensity = originalFireIntensity;
        }

        // revert agent and targeting
        if (agentRef != null)
        {
            agentRef.enabled = true;
        }
        if (targetingRef != null)
        {
            targetingRef.enabled = true;
        }
    }

    public class VaporizeTag : ElementTag
    {
        private float durationRemaining;
        private float tickInterval;
        private float tickTimer;
        private bool logTicks;
        private GameObject instigator;


        private int raycastMask;

        public float Intensity;

        public void SetContext(int mask)
        {
            raycastMask = mask;
        }

        public override void Apply(
            float duration,
            float interval,
            float intensity,
            bool refreshDuration,
            bool stackIntensity,
            GameObject instigator,
            bool logTicks)
        {
            tickInterval = Mathf.Max(0.05f, interval);
            this.logTicks = logTicks;

            if (stackIntensity)
            {
                Intensity += intensity;
            }
            else
            {
                Intensity = intensity;
            }

            if (refreshDuration || durationRemaining <= 0f)
            {
                durationRemaining = Mathf.Max(0.05f, duration);
            }

            tickTimer = tickInterval;
            this.instigator = instigator;
        }

        private void Update()
        {
            if (durationRemaining <= 0f)
            {
                RemoveOwners();
                return;
            }

            durationRemaining -= Time.deltaTime;
            tickTimer -= Time.deltaTime;

            if (tickTimer <= 0f)
            {
                tickTimer += tickInterval;
                if (Physics.Raycast(transform.position, Vector3.down, out var _, 1.2f, raycastMask))
                { 
                    RemoveOwners();
                    return;
                }

                if (logTicks)
                {
                    Debug.Log($"Vaporize tick on {gameObject.name} (intensity: {Intensity:F2})");
                }
            }
        }
    }
}