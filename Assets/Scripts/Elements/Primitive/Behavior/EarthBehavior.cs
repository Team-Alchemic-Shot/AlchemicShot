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

    public override Type TagType { get; } = typeof(EarthStatus);

    public override void Apply(ElementBehaviorContext context)
    {
        if (context.target == null)
        {
            return;
        }

        if (!context.target.TryGetComponent<EarthStatus>(out var status))
        {
            status = context.target.AddComponent<EarthStatus>();
        }

        status.SetContext(this, context); // sometimes additional context is needed
        status.Apply(
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
        if (context.target == null)
        {
            return;
        }

        if (context.target.TryGetComponent<EarthStatus>(out var status)
            && context.target.TryGetComponent<Health>(out var health))
        {
            var appliedWeakness = status.GetAppliedWeakness();
            if (appliedWeakness != 0f)
            {
                health.ApplyWeakness(-appliedWeakness);
            }

            status.ResetAppliedWeakness();
        }
    }

    private class EarthStatus : ElementTag
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
                if (sourceBehavior != null && lastContext.target != null)
                {
                    sourceBehavior.Remove(lastContext);
                }
                else
                {
                    Destroy(this);
                }
                return;
            }

            durationRemaining -= Time.deltaTime;
            tickTimer -= Time.deltaTime;

            if (tickTimer <= 0f)
            {
                tickTimer += tickInterval;
                if (logTicks)
                {
                    Debug.Log($"Earth tick on {gameObject.name} (intensity: {intensity:F2})");
                }
            }
        }
    }
}
