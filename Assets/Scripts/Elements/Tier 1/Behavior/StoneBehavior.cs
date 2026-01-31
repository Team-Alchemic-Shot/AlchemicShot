using System;
using UnityEngine;

[CreateAssetMenu(fileName = "StoneBehavior", menuName = "Elements/Behaviors/Tier 1/Stone")]

public class StoneBehavior : ElementBehavior
{
    public override Type StatusType { get; } = null;

    public override void Apply(ElementBehaviorContext context)
    {
        if (context.target.TryGetComponent<Health>(out var health))
        {
            Debug.Log($"StoneBehavior applied massive damage to {context.target.name}");
        }
    }
}