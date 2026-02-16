using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SandyBehavior", menuName = "Elements/Behaviors/Tier 1/Sandy")]
public class SandyBehavior : ElementBehavior
{
    [Header("Sandy Settings")]
    [SerializeField]
    private LayerMask enemyMask = 1;
    [SerializeField]
    private float range = 5f;
    [SerializeField]
    private WaterBehavior waterBehaviorTemplate;

    [Header("Water DOT Interaction")]
    [SerializeField]
    private float waterTickInterval = 0.5f;
    [SerializeField]
    private bool waterRefreshDuration = true;
    [SerializeField]
    private bool waterStackIntensity = false;
    [SerializeField]
    private bool waterLogTicks = true;
    [SerializeField]
    private float waterDuration = 10f;
    [SerializeField]
    private float waterDefaultIntensity = 10f;

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
            RemoveBehavior(context);
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

        // has secondary residual effect: track tag ownership with behavior template for cleanup on water tag removal
        EnsureTagOwner(context, waterTag, waterBehaviorTemplate); 
    }
}