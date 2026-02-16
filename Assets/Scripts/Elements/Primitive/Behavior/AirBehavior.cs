using UnityEngine;

[CreateAssetMenu(fileName = "AirBehavior", menuName = "Elements/Behaviors/Primitive/Air")]
public class AirBehavior : ElementBehavior
{
    public override void Apply(ElementBehaviorContext context)
    {
        if (context.Target == null)
        {
            return;
        }

        // Get the direction from instigator to target
        Vector3 knockbackDirection = (context.Target.transform.position - context.instigator.transform.position).normalized;

        // Apply knockback based on configuration
        if (context.Target.TryGetComponent<Rigidbody>(out var rigidbody))
        {
            rigidbody.AddForce(knockbackDirection * defaultIntensity, ForceMode.Impulse);
        }

        // remove since this is an instantaneous effect
        RemoveBehavior(context);
    }
}
