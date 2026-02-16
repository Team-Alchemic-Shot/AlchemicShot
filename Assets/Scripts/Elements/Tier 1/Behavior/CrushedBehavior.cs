using UnityEngine;

[CreateAssetMenu(fileName = "CrushedBehavior", menuName = "Elements/Behaviors/Tier 1/Crushed")]
public class CrushedBehavior : ElementBehavior // TODO refactor like Sandy?
{
    [SerializeField]
    private float tickInterval = 0.5f;
    [SerializeField]
    private bool refreshDuration = true;
    [SerializeField]
    private bool stackIntensity = false;
    [SerializeField]
    private bool logTicks = true;

    public override void Apply(ElementBehaviorContext context)
    {
        if (context.Target == null)
        {
            return;
        }

        // find or create tag instance
        var crushedTag = ElementTag.GetOrAddTag<CrushedTag>(context.Target); // could just piggyback off Earth
        TrackTag(crushedTag, context);

        // set context and apply tag
        crushedTag.SetContext(this, context);
        crushedTag.Apply(
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
        Debug.Log($"revert {nameof(EarthBehavior)} on {context.Target.name}");

        if (context.Target == null)
        {
            return;
        }

    
        if (context.Target.TryGetComponent<CrushedTag>(out var crushedTag)
            && context.Target.TryGetComponent<Health>(out var health))
        {
            var appliedWeakness = crushedTag.GetAppliedWeakness();
            if (appliedWeakness != 0f)
            {
                health.ApplyWeakness(-appliedWeakness);
            }

            crushedTag.ResetAppliedWeakness();
        }
    }
}

public class CrushedTag : ElementTag
{
    private float durationRemaining;
    private float tickInterval;
    private float tickTimer;
    private float intensity;
    private bool logTicks;

    private float currentAppliedWeakness;

    private ElementBehavior sourceBehavior;
    private ElementBehaviorContext lastContext;

    public void SetContext(ElementBehavior behavior, ElementBehaviorContext context)
    {
        sourceBehavior = behavior;
        lastContext = context;
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

        // apply weakness flat multiplier
        if (TryGetComponent<Health>(out var health))
        {
            var delta = this.intensity - currentAppliedWeakness;
            if (Mathf.Abs(delta) > 0f)
            {
                health.ApplyWeakness(delta);
                currentAppliedWeakness = this.intensity;
            }
        }
    }

    public float GetAppliedWeakness()
    {
        return currentAppliedWeakness;
    }

    public void ResetAppliedWeakness()
    {
        currentAppliedWeakness = 0f;
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

            if (logTicks)
            {
                Debug.Log($"Crushed tick on {gameObject.name} (intensity: {intensity:F2})");
            }
        }
    }
}
