using UnityEngine;

[CreateAssetMenu(fileName = "StoneBehavior", menuName = "Elements/Behaviors/Tier 1/Stone")]
public class StoneBehavior : ElementBehavior
{
    public override void Apply(ElementBehaviorContext context)
    {
        if (context.target.TryGetComponent<Health>(out var health))
        {
            health.ApplyDamage(new DamageInfo(1000f, context.instigator, context.target.transform.position));
            Debug.Log($"StoneBehavior applied massive damage to {context.target.name}");
        }
    }
}