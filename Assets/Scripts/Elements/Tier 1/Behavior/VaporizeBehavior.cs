using System.Collections.Generic;
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
    private FireBehavior.FireDOTTag fireTagRef;

    public override void Apply(ElementBehaviorContext context)
    {
        if (context.Target == null)
        {
            return;
        }

        // find nearby enemies (including the shot target)
        var center = context.Target.transform.position;
        var aoeTargets = AoeTargeting.CollectTargets(context.Target, center, range, enemyMask);
        if (aoeTargets.Count == 0)
        {
            // fallback to just the hit target
            ApplyToTarget(context);
            return;
        }

        // order targets with the primary target first, then apply behavior to all
        var orderedTargets = AoeTargeting.CreateOrderedTargetList(context.Target, aoeTargets);
        // mutate context to include all affected targets
        context.targets = new Queue<GameObject>(orderedTargets); 

        foreach (var target in aoeTargets)
        {
            // build unique context for each target
            var targetContext = new ElementBehaviorContext
            {
                targets = new Queue<GameObject>(new[] { target }),
                instigator = context.instigator,
                sourceBullet = context.sourceBullet,
                position = target.transform.position
            };

            if (target == context.Target)
            {
                // primary target uses the existing behavior instance
                // this is the context passed through the gun pipeline
                ApplyToTarget(context);
                continue;
            }

            // ensure aoe targets have their own behavior instance + status tracking
            var elementStatus = ElementStatus.GetOrCreateElementStatus(target);
            elementStatus.AddElement(targetContext.sourceBullet.element);

            // each target needs a unique behavior instance 
            var behaviorInstance = CreateRuntimeBehavior(this, targetContext, elementStatus, true);
            behaviorInstance.ApplyToTarget(targetContext);
        }
    }

    private void ApplyToTarget(ElementBehaviorContext context)
    {
        // get agent 
        if (!context.Target.TryGetComponent(out agentRef))
        {
            return;
        }

        // get targeting
        if (!context.Target.TryGetComponent(out targetingRef))
        {
            return;
        }
        targetingRef.enabled = false; // disable targeting becuase now the agent is disabled

        // throw enemy into the air
        if (!context.Target.TryGetComponent(out rigidbodyRef))
        {
            return;
        }
        originalIsKinematic = rigidbodyRef.isKinematic;

        agentRef.enabled = false; // needs to be disabled to leave the navmesh

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
        fireTagRef = ElementTag.GetOrAddTag<FireBehavior.FireDOTTag>(context.Target);
        // no tracking fire tag

        originalFireIntensity = fireTagRef.Intensity; // save for reversion

        fireTagRef.Apply( // reapply fire tag to recontextualize it with the vaporize context
            fireDuration, // will expire and auto clean up
            fireTickInterval,
            fireDefaultIntensity,
            fireRefreshDuration,
            fireStackIntensity,
            context.instigator,
            fireLogTicks);

        var vaporizeTag = ElementTag.GetOrAddTag<VaporizeTag>(context.Target);
        vaporizeTag.enabled = true;
        vaporizeTag.SetContext(floorMask);
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
        if (context.Target == null)
        {
            return;
        }

        // revert fire tag if it exists
        if (fireTagRef != null)
        {
            fireTagRef.Intensity = originalFireIntensity;
        }

        // revert agent and targeting
        if (rigidbodyRef != null)
        {
            rigidbodyRef.velocity = Vector3.zero;
            rigidbodyRef.angularVelocity = Vector3.zero;
            // handle kinematic to make sure not fighting physics
            rigidbodyRef.isKinematic = originalIsKinematic;
        }

        if (agentRef != null)
        {
            // snap onto navmesh before enabling movement/targeting again
            // if (NavMesh.SamplePosition(agent.transform.position, out var hit, 2f, NavMesh.AllAreas))
            // {
            //     agent.Warp(hit.position);
            // }

            agentRef.enabled = true;
            agentRef.ResetPath();
        }

        if (targetingRef != null)
        {
            targetingRef.enabled = true;
        }
    }

    public class VaporizeTag : ElementTag
    {
        
        private float durationRemaining;
        private float groundedRayDistance = 2.5f;
        private LayerMask raycastMask;


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
  
            // TODO Ignoring mask for now
            if (Physics.Raycast(transform.position, Vector3.down, out var _, groundedRayDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                Debug.Log($"[Vaporize] Tag grounded on {gameObject.name}, calling RemoveOwners()");
                enabled = false;
                RemoveOwners();
            }
        }
    }
}
