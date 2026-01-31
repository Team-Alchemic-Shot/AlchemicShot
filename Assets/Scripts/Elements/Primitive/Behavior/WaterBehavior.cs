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

        if (!context.target.TryGetComponent<WaterDotStatus>(out var status))
        {
            status = context.target.AddComponent<WaterDotStatus>();
        }


        status.Apply(duration, tickInterval, defaultIntensity, refreshDuration, stackIntensity, logTicks);
    }

    private class WaterDotStatus : MonoBehaviour
    {
        private float durationRemaining;
        private bool logTicks;
        private bool hasAppliedSlowness = false;
        private float tickInterval;
        private float tickTimer;

        public void Apply(float duration, float interval, float intensity, bool refreshDuration, bool stackIntensity, bool logTicks)
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
                agent.speed *= 1 / intensity; // intensity is inverse of speed multiplier
                hasAppliedSlowness = true;

                if (logTicks)
                {
                    Debug.Log($"Water slow applied to {gameObject.name} ({(1 - (1 / intensity)) * 100}% speed reduction)");
                }
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
                    Debug.Log($"Water tick on {gameObject.name}");
                }
            }
        }
    }
}
