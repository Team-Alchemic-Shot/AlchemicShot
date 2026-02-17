using UnityEngine;

[CreateAssetMenu(fileName = "CrushedBehavior", menuName = "Elements/Behaviors/Tier 1/Crushed")]
public class CrushedBehavior : ElementBehavior
{
    [Header("Earth DOT Interaction")]
    [SerializeField]    
    private float earthTickInterval = 0.5f;
    [SerializeField]
    private float earthDuration = 10f;
    [SerializeField]
    private float earthDefaultIntensity = 10f;
    [SerializeField]
    private bool earthRefreshDuration = true;
    [SerializeField]
    private bool earthStackIntensity = false;
    [SerializeField]
    private bool earthLogTicks = true;
    [SerializeField]
    private EarthBehavior earthBehaviorTemplate;

    public override void Apply(ElementBehaviorContext context)
    {
        if (context.Target == null)
        {
            return;
        }

        // find or create tag instance
        var earthTag = ElementTag.GetOrAddTag<EarthBehavior.EarthTag>(context.Target);
        earthTag.Apply(
            earthDuration,
            earthTickInterval,
            earthDefaultIntensity,
            earthRefreshDuration,
            earthStackIntensity,
            context.instigator,
            earthLogTicks);
        EnsureTagOwner(context, earthTag, earthBehaviorTemplate); // secondary residuals    
        
        // remove since this is an instantaneous effect
        RemoveBehavior(context);
    }
}
