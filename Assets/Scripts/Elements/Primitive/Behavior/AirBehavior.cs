using UnityEngine;

[CreateAssetMenu(fileName = "AirBehavior", menuName = "Elements/Behaviors/Air")]
public class AirBehavior : ElementBehavior
{
    [SerializeField]
    private float knockbackForce = 10f;
    [SerializeField]
    private float knockbackDistance = 5f;
    [SerializeField]
    private bool useForce = true;

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
            if (useForce)
            {
                rigidbody.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
            }
            else
            {
                // Alternative: move the object directly
                context.target.transform.position += knockbackDirection * knockbackDistance;
            }
        }
        else if (context.target.TryGetComponent<Rigidbody2D>(out var rigidbody2D))
        {
            if (useForce)
            {
                rigidbody2D.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            }
            else
            {
                // Alternative: move the object directly
                context.target.transform.position += knockbackDirection * knockbackDistance;
            }
        }
        else
        {
            // Fallback: move without physics
            context.target.transform.position += knockbackDirection * knockbackDistance;
        }

        Debug.Log($"Knockback applied to {context.target.name} with force {knockbackForce}");
    }
}
