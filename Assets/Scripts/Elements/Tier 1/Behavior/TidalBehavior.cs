using System;
using UnityEngine;

[CreateAssetMenu(fileName = "TidalBehavior", menuName = "Elements/Behaviors/Tier 1/Tidal")]
public class TidalBehavior : ElementBehavior
{
    public override Type TagType { get; } = null;

    public override void Apply(ElementBehaviorContext context)
    {
        // piggyback off existing water and air behavior to apply a knockback and slow effect
        // this behavior file technically doesn't need to exist for this behavior and reactions to function correctly,
        // but this file makes the python tool shut up and serves as a placeholder for any future unique behavior we might want to add to tidal element
    }
}

