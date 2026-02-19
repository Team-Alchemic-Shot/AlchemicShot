using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "FrostBehavior", menuName = "Elements/Behaviors/Tier 1/Frost")]
public class FrostBehavior : ElementBehavior
{
    private float existingSpeed = 1f;

    public override void Apply(ElementBehaviorContext context)
    {
        if (context.Target == null)
        {
            return;
        }

        // Apply frost effect (e.g., slow down movement)
        if (!context.Target.TryGetComponent<NavMeshAgent>(out var agent))
        {
            return;
        }

        existingSpeed = agent.speed;
        agent.speed = 0; // freeze movement

        // remove since this is an instantaneous effect
        RemoveBehavior(context);
    }

    public override void RevertEffects(ElementBehaviorContext context)
    {
        if (context.Target == null)
        {
            return;
        }

        // Revert frost effect
        if (context.Target.TryGetComponent<NavMeshAgent>(out var agent))
        {
            agent.speed = existingSpeed; // reset to original speed
        }
    }
}