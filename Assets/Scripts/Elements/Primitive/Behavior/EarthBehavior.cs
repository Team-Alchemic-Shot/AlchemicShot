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

    public override Type StatusType { get; } = typeof(EarthStatus);

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

        status.Apply(
            duration, 
            tickInterval, 
            defaultIntensity, 
            refreshDuration, 
            stackIntensity, 
            context.instigator, 
            logTicks);
    }

    private class EarthStatus : MonoBehaviour
    {
        private float durationRemaining;
        private float tickInterval;
        private float tickTimer;
        private float intensity;
        private bool logTicks;

        public void Apply(
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
                health.ApplyWeakness(this.intensity);
            }
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
                    Debug.Log($"Earth tick on {gameObject.name} (intensity: {intensity:F2})");
                }
            }
        }
    }
}
