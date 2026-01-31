using UnityEngine;

[CreateAssetMenu(fileName = "AirBehavior", menuName = "Elements/Behaviors/Air")]
public class AirBehavior : ElementBehavior
{
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
            Debug.Log($"Knockback applied to {context.target.name} with force {defaultIntensity}");
        }
    }
}
