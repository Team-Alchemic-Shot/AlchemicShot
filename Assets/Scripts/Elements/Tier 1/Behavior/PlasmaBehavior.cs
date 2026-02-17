using UnityEngine;

[CreateAssetMenu(fileName = "PlasmaBehavior", menuName = "Elements/Behaviors/Tier 1/Plasma")]
public class PlasmaBehavior : ElementBehavior
{
    [Header("Plasma DOT Settings")]
    [SerializeField]
    private float tickInterval = 0.5f;
    [SerializeField]
    private bool refreshDuration = true;
    [SerializeField]
    private bool stackIntensity = false;
    [SerializeField]
    private bool logTicks = true;

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

    private float existingIntensity;

    public override void Apply(ElementBehaviorContext context)
    {
        // get fire tag instance
        var fireTag = ElementTag.GetOrAddTag<FireBehavior.FireDOTTag>(context.Target);
        existingIntensity = fireTag.Intensity; // save for reverting later
        fireTag.Apply( // reapply fire tag to recontextualize it with the plasma's context
            fireDuration, // will expire and auto clean up
            fireTickInterval,
            fireDefaultIntensity,
            fireRefreshDuration,
            fireStackIntensity,
            context.instigator,
            fireLogTicks);

        // find or create tag instance
        var plasmaTag = ElementTag.GetOrAddTag<PlasmaTag>(context.Target);
        TrackTag(plasmaTag, context);

        // set context and apply tag
        plasmaTag.SetContext(fireTag);
        plasmaTag.Apply(
            duration,
            tickInterval,
            defaultIntensity,
            refreshDuration,
            stackIntensity,
            context.instigator,
            logTicks);
    }

    public override void RevertEffects(ElementBehaviorContext context)
    {
        // get fire tag instance
        if (!context.Target.TryGetComponent<ElementStatus>(out var status))
        {
            return;
        }
        if (!status.TryGetTag<FireBehavior.FireDOTTag>(out var fireTag))
        {
            return; // fire may already be gone, in which case we can just skip reverting plasma effects
        }

        // revert to original intensity before plasma applied
        fireTag.Intensity = existingIntensity;
    }
}

public class PlasmaTag : ElementTag
{
    private float durationRemaining;
    private float tickInterval;
    private float tickTimer;
    private float intensity;
    private bool logTicks;
    private FireBehavior.FireDOTTag fireTag;

    public void SetContext(FireBehavior.FireDOTTag fireTag)
    {
        this.fireTag = fireTag;
    }

    public override void Apply(
        float duration,
        float interval,
        float intensity,
        bool refreshDuration,
        bool stackIntensity,
        GameObject instigator,
        bool logTicks)
    {
        tickInterval = Mathf.Max(0.05f, interval);
        this.logTicks = logTicks;

        if (stackIntensity)
        {
            this.intensity += intensity;
        }
        else
        {
            this.intensity = intensity;
        }

        if (refreshDuration || durationRemaining <= 0f)
        {
            durationRemaining = Mathf.Max(0.05f, duration);
        }

        tickTimer = tickInterval;
    }

    private void Update()
    {
        if (durationRemaining <= 0f)
        {
            RemoveOwners();
            return;
        }

        durationRemaining -= Time.deltaTime;
        tickTimer -= Time.deltaTime;

        if (tickTimer <= 0f)
        {
            tickTimer += tickInterval;

            // plasma DOT formula
            fireTag.Intensity += intensity;

            if (logTicks)
            {
                Debug.Log($"Plasma DOT tick on {gameObject.name} (intensity: {intensity:F2})");
            }
        }
    }
}
