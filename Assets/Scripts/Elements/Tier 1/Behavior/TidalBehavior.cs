using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TidalBehavior", menuName = "Elements/Behaviors/Tier 1/Tidal")]
public class TidalBehavior : ElementBehavior
{
    
    [SerializeField]
    private LayerMask enemyMask = 1;

    [SerializeField]
    private float range = 5f;

    [Header("Knockback")]
    [SerializeField]
    private float maxAngle = 15f; //scatter is angle now

    
    [SerializeField]
    private WaterBehavior waterBehaviorTemplate;

    [Header("Tidal Water DOT Interaction")]
    [SerializeField]
    private float waterTickInterval = 0.5f;

    [SerializeField]
    private bool waterRefreshDuration = true;

    [SerializeField]
    private bool waterStackIntensity = false;

    [SerializeField]
    private bool waterLogTicks = true;

    [SerializeField]
    private float waterDuration = 6f;

    [SerializeField]
    private float waterDefaultIntensity = 12f;

    public override void Apply(ElementBehaviorContext context)
    {
        if (context.Target == null)
        {
            return;
        }


        // find nearby enemies 
        var center = context.Target.transform.position;
        var aoeTargets = AoeTargeting.CollectTargets(context.Target, center, range, enemyMask);
        if (aoeTargets.Count == 0)
        {
            // fallback to just the hit target
            ApplyToTarget(context);
            RemoveBehavior(context);
            return;
        }

        // order targets with the primary target first, then apply behavior to all
        var orderedTargets = AoeTargeting.CreateOrderedTargetList(context.Target, aoeTargets);
        //  include all affected targets
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
                RemoveBehavior(context);
                continue;
            }

            // ensure aoe targets have their own behavior instance + status tracking
            var elementStatus = ElementStatus.GetOrCreateElementStatus(target);
            elementStatus.AddElement(targetContext.sourceBullet.element);

            // each target needs a unique behavior instance
            var behaviorInstance = CreateRuntimeBehavior(this, targetContext, elementStatus, true);
            behaviorInstance.ApplyToTarget(targetContext);
            behaviorInstance.RemoveBehavior(targetContext);
        }
    }

    private void ApplyToTarget(ElementBehaviorContext context)
    {
        if (context.Target == null)
        {
            return;
        }

        // apply water tag
        var waterTag = ElementTag.GetOrAddTag<WaterBehavior.WaterDOTTag>(context.Target);
        waterTag.Apply(
            waterDuration,
            waterTickInterval,
            waterDefaultIntensity,
            waterRefreshDuration,
            waterStackIntensity,
            context.instigator,
            waterLogTicks);

        EnsureTagOwner(context, waterTag, waterBehaviorTemplate);

        // knockback
        if (!context.Target.TryGetComponent<Rigidbody>(out var rigidbody))
        {
            return;
        }

        if (context.instigator == null)
        {
            return;
        }

        // preferred direction is away from instigator to  target
        var preferredDir = context.Target.transform.position - context.instigator.transform.position;
        preferredDir.y = 0f;

        if (preferredDir.sqrMagnitude < 0.0001f)
        {
            return;
        }

        // maxAngle in degrees.
        
        var finalDir = ElementDirection.BiasedDirection(preferredDir, maxAngle);

        rigidbody.AddForce(finalDir * defaultIntensity, ForceMode.Impulse);
    }



}
