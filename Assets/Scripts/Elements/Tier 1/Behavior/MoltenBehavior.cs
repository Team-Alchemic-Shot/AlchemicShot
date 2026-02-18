using UnityEngine;

[CreateAssetMenu(fileName = "MoltenBehavior", menuName = "Elements/Behaviors/Tier 1/Molten")]
public class MoltenBehavior : ElementBehavior
{
    [Header("Fire DOT Interaction")]
    [SerializeField]
    private float fireTickInterval = 0.5f;
    [SerializeField]
    private bool fireRefreshDuration = true;
    [SerializeField]
    private bool fireStackIntensity = false;
    [SerializeField]
    private bool fireLogTicks = true;
    [SerializeField]
    private float fireDuration = 3f;
    [SerializeField]
    private float fireDefaultIntensity = 15f;

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
        var fireTag = ElementTag.GetOrAddTag<FireBehavior.FireDOTTag>(context.Target);
        fireTag.Apply(
            fireDuration,
            fireTickInterval,
            fireDefaultIntensity,
            fireRefreshDuration,
            fireStackIntensity,
            context.instigator,
            fireLogTicks);
        // no secondary residuals, so no need to ensure tag ownership

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