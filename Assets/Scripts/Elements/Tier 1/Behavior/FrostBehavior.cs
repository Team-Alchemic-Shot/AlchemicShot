using UnityEngine;

[CreateAssetMenu(fileName = "FrostBehavior", menuName = "Elements/Behaviors/Tier 1/Frost")]
public class FrostBehavior : ElementBehavior
{
    public override void Apply(ElementBehaviorContext context)
    {
        if (context.Target == null)
        {
            return;
        }

        if (!context.Target.TryGetComponent<ZombieSpeedController>(out var speed))
        {
            return;
        }

        speed.AddFreeze(this);

        // remove since this is an instantaneous effect
        RemoveBehavior(context);
    }

    public override void RevertEffects(ElementBehaviorContext context)
    {
        if (context.Target == null)
        {
            return;
        }

        if (context.Target.TryGetComponent<ZombieSpeedController>(out var speed))
        {
            speed.RemoveFreeze(this);
        }
    }
}