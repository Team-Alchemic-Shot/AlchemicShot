using UnityEngine;

[CreateAssetMenu(fileName = "MuddyBehavior", menuName = "Elements/Behaviors/Tier 1/Muddy")]
public class MuddyBehavior : ElementBehavior
{
    [Header("Water DOT Interaction")]
    [SerializeField]
    private float waterTickInterval = 0.5f;
    [SerializeField]
    private bool waterRefreshDuration = true;
    [SerializeField]
    private bool waterStackIntensity = false;
    [SerializeField]
    private bool waterLogTicks = true;
    [SerializeField]
    private float waterDuration = 10f;
    [SerializeField]
    private float waterDefaultIntensity = 10f;
    [SerializeField]
    private WaterBehavior waterBehaviorTemplate;

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

        // apply slow and weak
        // both have secondary residuals that need proper behavior ownership, so templates are needed
        var waterTag = ElementTag.GetOrAddTag<WaterBehavior.WaterDOTTag>(context.Target);
        waterTag.Apply(
            waterDuration,
            waterTickInterval,
            waterDefaultIntensity,
            waterRefreshDuration,
            waterStackIntensity,
            context.instigator,
            waterLogTicks);
        EnsureTagOwner(context, waterTag, waterBehaviorTemplate); 

        var earthTag = ElementTag.GetOrAddTag<EarthBehavior.EarthTag>(context.Target);
        earthTag.Apply(
            earthDuration,
            earthTickInterval,
            earthDefaultIntensity,
            earthRefreshDuration,
            earthStackIntensity,
            context.instigator,
            earthLogTicks);
        EnsureTagOwner(context, earthTag, earthBehaviorTemplate);

        // remove since this is an instantaneous effect
        RemoveBehavior(context);
    }
}