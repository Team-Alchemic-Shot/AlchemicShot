using System;
using UnityEngine;

[CreateAssetMenu(fileName = "AirBehavior", menuName = "Elements/Behaviors/Primitive/Air")]
public class AirBehavior : ElementBehavior
{
    public override Type TagType { get; } = null;

    public override void Apply(ElementBehaviorContext context)
    {
        if (context.target == null)
        {
            return;
        }

        // Get the direction from instigator to target
        Vector3 knockbackDirection = (context.target.transform.position - context.instigator.transform.position).normalized;

        // Apply knockback based on configuration
        if (context.target.TryGetComponent<Rigidbody>(out var rigidbody))
        {
            rigidbody.AddForce(knockbackDirection * defaultIntensity, ForceMode.Impulse);
        }

        // remove since this is an instantaneous effect
        RemoveBehavior(context);
    }
}
