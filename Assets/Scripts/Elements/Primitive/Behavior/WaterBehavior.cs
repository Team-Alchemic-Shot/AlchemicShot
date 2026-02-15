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

    public override void Apply(ElementBehaviorContext context)
    {
        if (context.target == null)
        {
            return;
        }

        if (!context.target.TryGetComponent<WaterDOTTag>(out var tag))
        {
            tag = context.target.AddComponent<WaterDOTTag>();
        }
        TrackTag(tag, context);

        tag.Apply(duration, tickInterval, defaultIntensity, refreshDuration, stackIntensity, context.instigator, logTicks);
    }

    public override void RevertEffects(ElementBehaviorContext context)
    {
        if (context.target == null)
        {
            return;
        }

        if (context.target.TryGetComponent<WaterDOTTag>(out var tag)
            && context.target.TryGetComponent<NavMeshAgent>(out var agent))
        {
            var appliedMultiplier = tag.GetAppliedSpeedMultiplier();
            if (appliedMultiplier != 0f && appliedMultiplier != 1f)
            {
                agent.speed /= appliedMultiplier;
            }

            tag.ResetAppliedSpeedMultiplier();
        }
    }

    private class WaterDOTTag : ElementTag
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
                var desiredMultiplier = 1 / intensity; // intensity is inverse of speed multiplier
                appliedSpeedMultiplier = desiredMultiplier;
                agent.speed *= appliedSpeedMultiplier;
                hasAppliedSlowness = true;

                if (logTicks)
                {
                    Debug.Log($"Water slow applied to {gameObject.name} ({(1 - (1 / intensity)) * 100}% speed reduction)");
                }
            }
            else
            {
                var desiredMultiplier = 1 / intensity;
                if (appliedSpeedMultiplier != 0f)
                {
                    var ratio = desiredMultiplier / appliedSpeedMultiplier;
                    agent.speed *= ratio;
                    appliedSpeedMultiplier = desiredMultiplier;
                }
            }
        }

        public float GetAppliedSpeedMultiplier()
        {
            return appliedSpeedMultiplier;
        }

        public void ResetAppliedSpeedMultiplier()
        {
            appliedSpeedMultiplier = 1f;
            hasAppliedSlowness = false;
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
                    Debug.Log($"Water tick on {gameObject.name}");
                }
            }
        }
    }
}
