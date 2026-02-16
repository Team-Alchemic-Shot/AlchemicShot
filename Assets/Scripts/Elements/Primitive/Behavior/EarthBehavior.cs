using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EarthBehavior", menuName = "Elements/Behaviors/Primitive/Earth")]
public class EarthBehavior : ElementBehavior
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

        var tag = ElementTag.GetOrAddTag<EarthTag>(context.Target);
        TrackTag(tag, context);

        tag.Apply(
            duration, 
            tickInterval, 
            defaultIntensity, 
            refreshDuration, 
            stackIntensity, 
            context.instigator, 
            logTicks);
            Debug.Log($"earth applied on {context.Target.name} duration={duration} tick={tickInterval} logTicks={logTicks}");

    }

    public override void RevertEffects(ElementBehaviorContext context)
    {
        Debug.Log($"revert {nameof(EarthBehavior)} on {context.Target.name}");

        if (context.Target == null)
        {
            return;
        }

        if (context.Target.TryGetComponent<EarthTag>(out var tag)
            && context.Target.TryGetComponent<Health>(out var health))
        {
            var appliedWeakness = tag.GetAppliedWeakness();
            if (appliedWeakness != 0f)
            {
                health.ApplyWeakness(-appliedWeakness);
            }

            tag.ResetAppliedWeakness();
        }
    }

    public class EarthTag : ElementTag // TODO this tag is weird and probably needs redone
    {
        private float durationRemaining;
        private float tickInterval;
        private float tickTimer;
        private float intensity;
        private bool logTicks;
        private float currentAppliedWeakness;


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
                    Debug.Log($"earth tick on {gameObject.name} (intensity: {intensity:F2})");
                }
            }
        }
    }
}
