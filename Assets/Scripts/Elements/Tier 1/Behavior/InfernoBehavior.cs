using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InfernoBehavior", menuName = "Elements/Behaviors/Tier 1/Inferno")]
public class InfernoBehavior : ElementBehavior
{
    [Header("Inferno Settings")]
    [SerializeField]
    private LayerMask enemyMask = 1;
    [SerializeField]
    private float range = 5f;

    [Header("Fire DOT Interaction")]
    [SerializeField]
    private float fireTickInterval = 0.5f;
    [SerializeField]
    private bool fireRefreshDuration = true;
    [SerializeField]
    private bool fireStackIntensity = false;
    [SerializeField]
    private bool fireLogTicks = true;
    [SerializeField]
    private float fireDuration = 3f;
    [SerializeField]
    private float fireDefaultIntensity = 15f;

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
                RemoveBehavior(context); 
                continue;
            }

            // ensure aoe targets have their own behavior instance + status tracking
            var elementStatus = ElementStatus.GetOrCreateElementStatus(target);
            elementStatus.AddElement(targetContext.sourceBullet.element);

            // each target needs a unique behavior instance 
            var behaviorInstance = CreateRuntimeBehavior(this, targetContext, elementStatus);
            behaviorInstance.ApplyToTarget(targetContext);
            behaviorInstance.RemoveBehavior(context);
        }
    }

    private void ApplyToTarget(ElementBehaviorContext context)
    {
        if (context.Target == null)
        {
            return;
        }

        // apply fire tag
        var fireTag = ElementTag.GetOrAddTag<FireBehavior.FireDOTTag>(context.Target);
        fireTag.Apply(
            fireDuration,
            fireTickInterval,
            fireDefaultIntensity,
            fireRefreshDuration,
            fireStackIntensity,
            context.instigator,
            fireLogTicks);

        // don't own tag so it can outlast the inferno behavior
    }

    public override void RevertEffects(ElementBehaviorContext context)
    {
        // TODO should we need to remove the fire tag?
    }
}