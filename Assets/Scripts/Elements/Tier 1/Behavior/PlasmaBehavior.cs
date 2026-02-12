using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PlasmaBehavior", menuName = "Elements/Behaviors/Tier 1/Plasma")]
public class PlasmaBehavior : ElementBehavior
{
    [SerializeField]
    private float tickInterval = 0.5f;
    [SerializeField]
    private bool refreshDuration = true;
    [SerializeField]
    private bool stackIntensity = false;
    [SerializeField]
    private bool logTicks = true;

    private float existingIntensity;

    public override Type TagType { get; } = typeof(PlasmaTag);

    public override void Apply(ElementBehaviorContext context)
    {
        // get fire tag instance
        if (!context.target.TryGetComponent<ElementStatus>(out var status))
        {
            return;
        }
        if (!status.TryGetTag<FireBehavior.FireDotStatus>(out var fireTag))
        {
            return;
        }
        existingIntensity = fireTag.Intensity; // save for reverting later

        // find or create tag instance
        if (!context.target.TryGetComponent<PlasmaTag>(out var plasmaTag))
        {
            plasmaTag = context.target.AddComponent<PlasmaTag>();
        }

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
        if (!context.target.TryGetComponent<ElementStatus>(out var status))
        {
            return;
        }
        if (!status.TryGetTag<FireBehavior.FireDotStatus>(out var fireTag))
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
    private FireBehavior.FireDotStatus fireStatus;

    public void SetContext(FireBehavior.FireDotStatus fireStatus)
    {
        this.fireStatus = fireStatus;
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
            Destroy(this);
            return;
        }

        durationRemaining -= Time.deltaTime;
        tickTimer -= Time.deltaTime;

        if (tickTimer <= 0f)
        {
            tickTimer += tickInterval;

            // plasma DOT formula
            fireStatus.Intensity += intensity;

            if (logTicks)
            {
                Debug.Log($"Plasma DOT tick on {gameObject.name} (intensity: {intensity:F2})");
            }
        }
    }
}
