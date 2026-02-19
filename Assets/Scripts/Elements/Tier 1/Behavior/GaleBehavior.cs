using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GaleBehavior", menuName = "Elements/Behaviors/Tier 1/Gale")]
public class GaleBehavior : ElementBehavior
{
    [Header("Gale Settings")]
    [SerializeField]
    private LayerMask enemyMask = 1;
    [SerializeField]
    private float range = 5f;
    [SerializeField]
    private float maxAngle = 30f; //scatter is angle now

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
            ApplyToTarget(context, center);
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
                ApplyToTarget(context, center);
                RemoveBehavior(context); 
                continue;
            }

            // ensure aoe targets have their own behavior instance + status tracking
            var elementStatus = ElementStatus.GetOrCreateElementStatus(target);
            elementStatus.AddElement(targetContext.sourceBullet.element);

            // each target needs a unique behavior instance 
            var behaviorInstance = CreateRuntimeBehavior(this, targetContext, elementStatus, true);
            behaviorInstance.ApplyToTarget(targetContext, center);
            
            behaviorInstance.RemoveBehavior(targetContext);

        }
    }

    private void ApplyToTarget(ElementBehaviorContext context, Vector3 center)
    {
        if (context.Target == null)
        {
            return;
        }

        if (!context.Target.TryGetComponent<Rigidbody>(out var rigidbody))
        {
            return;
        }

        if (context.instigator == null)
        {
            return;
        }

        // preferred direction is away from instigator to target
        var preferredDir = context.Target.transform.position - context.instigator.transform.position;
        preferredDir.y = 0f;

        if (preferredDir.sqrMagnitude < 0.0001f)
        {
            return;
        }

        //  maxAngle in degrees.
        var finalDir = ElementDirection.BiasedDirection(preferredDir, maxAngle);

        rigidbody.AddForce(finalDir * defaultIntensity, ForceMode.Impulse);
    }

}
