using System;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "StoneBehavior", menuName = "Elements/Behaviors/Tier 1/Stone")]

public class StoneBehavior : ElementBehavior
{
    public override Type TagType { get; } = null;

    public override void Apply(ElementBehaviorContext context)
    {
        if (context.target.TryGetComponent<NavMeshAgent>(out var agent))
        {
            agent.enabled = false;
        }
        if (context.target.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.AddForce(new(0, defaultIntensity, 0), ForceMode.Impulse);
        }

        RemoveBehavior(context);
    }

    // Note: this is a hacky way to revert since agent's need to be re-enabled close to a NavMesh surface.
    // probably better to build revert logic into a tag which is a MonoBehaviour (update checks for proximity to NavMesh)
    public override void RevertEffects(ElementBehaviorContext context)
    {
        if (context.target.TryGetComponent<NavMeshAgent>(out var agent))
        {
            agent.enabled = true;
        }
    }
}