using UnityEngine;

[CreateAssetMenu(fileName = "TidalBehavior", menuName = "Elements/Behaviors/Tier 1/Tidal")]
public class TidalBehavior : ElementBehavior
{
    public override void Apply(ElementBehaviorContext context)
    {
        // piggyback off existing water and air behavior to apply a knockback and slow effect
        RemoveBehavior(context);
    }
}

