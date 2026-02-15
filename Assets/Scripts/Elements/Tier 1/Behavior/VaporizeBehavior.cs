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

    private Rigidbody rigidbodyRef;
    private bool originalIsKinematic;

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
        if (context.target == null)
        {
            return;
        }

        // gather  
        if (!context.target.TryGetComponent(out NavMeshAgent agent))
        {
            return;
        }
        if (!context.target.TryGetComponent(out ZombieTargeting targeting))
        {
            return;
        }
        if (!context.target.TryGetComponent<Rigidbody>(out var rigidbody))
        {
            return;
        }

        // store refs for reversion
        agentRef = agent;
        targetingRef = targeting;
        rigidbodyRef = rigidbody;
        originalIsKinematic = rigidbodyRef.isKinematic;

        // get agent 
        agentRef.enabled = false; // needs to be disabled to leave the navmesh

        // get targeting
        targetingRef.enabled = false; // disable targeting 

        // make sure physics can move it 
        rigidbodyRef.isKinematic = false;
        rigidbodyRef.velocity = Vector3.zero;
        rigidbodyRef.angularVelocity = Vector3.zero;

        // throw enemy into the air
        var lateral = Random.insideUnitSphere;
        lateral.y = 0f;
        if (lateral.sqrMagnitude > 0.0001f)
        {
            lateral.Normalize();
        }
        rigidbodyRef.AddForce(Vector3.up * defaultIntensity + lateral * lateralImpulse, ForceMode.Impulse); // default intensity is used as the jump strength

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

        vaporizeTag.enabled = true;
        vaporizeTag.SetContext(floorMask);

        // TrackTag! 
        TrackTag(vaporizeTag, context);

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
        if (context.target.TryGetComponent<Rigidbody>(out var rigidbody))
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            // handle kinematic to make sure not fighting physics
            rigidbody.isKinematic = true;
        }

        if (context.target.TryGetComponent<NavMeshAgent>(out var agent))
        {
            // snap onto navmesh before enabling movement/targeting again
            // if (NavMesh.SamplePosition(agent.transform.position, out var hit, 2f, NavMesh.AllAreas))
            // {
            //     agent.Warp(hit.position);
            // }

            agent.enabled = true;
            agent.ResetPath();
        }

        if (context.target.TryGetComponent<ZombieTargeting>(out var targeting))
        {
            targeting.enabled = true;
        }

        
    }

    public class VaporizeTag : ElementTag
    {
        
        private float durationRemaining;
        private float groundedRayDistance = 2.5f;
        private LayerMask raycastMask;

        //  ElementTag
        private float interval;
        private float intensity;
        private bool logTicks;
        private GameObject instigator;

        private bool isConfigured;

        // prevent immediately grounding on the same frame
        private float groundedCheckDelay = 0.15f;

       
        //floormask
        public void SetContext(LayerMask floorMask)
        {
            raycastMask = floorMask;
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
            groundedCheckDelay = 0.15f;

            
            this.interval = interval;
            this.logTicks = logTicks;
            this.instigator = instigator;

            if (stackIntensity)
            {
                this.intensity += intensity;
            }
            else
            {
                this.intensity = intensity;
            }

            // refresh duration for vaporize 
            durationRemaining = duration;
            isConfigured = true;
        }

        private void Update()
        {
            if (!isConfigured)
            {
                return;
            }

            durationRemaining -= Time.deltaTime;

            // expire 
            if (durationRemaining <= 0f)
            {
                enabled = false;
                RemoveOwners();
                return;
            }

            groundedCheckDelay -= Time.deltaTime;
            if (groundedCheckDelay > 0f)
            {
                return;
            }

             
            // Ignoring mask for now
            if (Physics.Raycast(transform.position, Vector3.down, out var _, groundedRayDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                Debug.Log($"[Vaporize] Tag grounded on {gameObject.name}, calling RemoveOwners()");
                enabled = false;
                RemoveOwners();
            }
        }

    }
}
