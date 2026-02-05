using System;
using UnityEngine;

[CreateAssetMenu(fileName = "StoneBehavior", menuName = "Elements/Behaviors/Tier 1/Stone")]

public class StoneBehavior : ElementBehavior
{
    public override Type TagType { get; } = null;

    public override void Apply(ElementBehaviorContext context)
    {
        if (context.target.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.AddForce(new(0, 100, 0), ForceMode.Impulse);
            Debug.Log($"StoneBehavior applied: Added upward force to {context.target.name}.");
        }

        RemoveBehaviorAfterDelay(context, duration);
    }
}