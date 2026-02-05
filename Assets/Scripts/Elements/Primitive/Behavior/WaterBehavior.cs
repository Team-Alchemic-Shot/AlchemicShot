using System;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "WaterBehavior", menuName = "Elements/Behaviors/Primitive/Water")]
public class WaterBehavior : ElementBehavior
{
    [SerializeField]
    private float tickInterval = 0.5f;
    [SerializeField]
    private bool refreshDuration = true;
    [SerializeField]
    private bool stackIntensity = false;
    [SerializeField]
    private bool logTicks = true;

    public override Type TagType { get; } = typeof(WaterDotStatus);

    public override void Apply(ElementBehaviorContext context)
    {
        if (context.target == null)
        {
            return;
        }

        if (!context.target.TryGetComponent<WaterDotStatus>(out var status))
        {
            status = context.target.AddComponent<WaterDotStatus>();
        }

        status.Apply(duration, tickInterval, defaultIntensity, refreshDuration, stackIntensity, context.instigator, logTicks);
    }

    public override void RevertEffects(ElementBehaviorContext context)
    {
        // Reverted inside WaterDotStatus.OnDestroy using the actual applied multiplier.
    }

    private class WaterDotStatus : ElementTag
    {
        private float durationRemaining;
        private bool logTicks;
        private bool hasAppliedSlowness = false;
        private float tickInterval;
        private float tickTimer;
        private float appliedSpeedMultiplier = 1f;

        public override void Apply(
            float duration, 
            float interval, 
            float intensity, 
            bool refreshDuration, 
            bool stackIntensity,
            GameObject instigator, 
            bool logTicks)
        {
            if (!TryGetComponent<NavMeshAgent>(out var agent))
            {
                return;
            }

            this.logTicks = logTicks;
            tickInterval = Mathf.Max(0.05f, interval);

            if (stackIntensity)
            {
                intensity += intensity;
            }
            else 
            {
                intensity = Mathf.Max(20, intensity);
            }

            if (refreshDuration || durationRemaining <= 0f)
            {
                durationRemaining = Mathf.Max(0.05f, duration);
            }

            tickTimer = tickInterval;

            if (!hasAppliedSlowness)
            {
                appliedSpeedMultiplier = 1 / intensity; // intensity is inverse of speed multiplier
                agent.speed *= appliedSpeedMultiplier;
                hasAppliedSlowness = true;

                if (logTicks)
                {
                    Debug.Log($"Water slow applied to {gameObject.name} ({(1 - (1 / intensity)) * 100}% speed reduction)");
                }
            }
        }

        protected override void OnDestroy()
        {
            if (TryGetComponent<NavMeshAgent>(out var agent) && hasAppliedSlowness)
            {
                if (appliedSpeedMultiplier != 0f)
                {
                    agent.speed /= appliedSpeedMultiplier;
                }
            }
            base.OnDestroy();
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
                if (logTicks)
                {
                    Debug.Log($"Water tick on {gameObject.name}");
                }
            }
        }
    }
}
