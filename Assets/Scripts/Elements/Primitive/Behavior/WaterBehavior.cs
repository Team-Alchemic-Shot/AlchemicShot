using UnityEngine;

[CreateAssetMenu(fileName = "WaterBehavior", menuName = "Elements/Behaviors/Water")]
public class WaterBehavior : ElementBehavior
{
    [SerializeField]
    private float duration = 4f;
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

        float intensity = context.intensity <= 0f ? 1f : context.intensity;
        status.Apply(duration, tickInterval, intensity, refreshDuration, stackIntensity, logTicks);
    }

    private class WaterDotStatus : MonoBehaviour
    {
        private float durationRemaining;
        private bool logTicks;
        private Rigidbody2D rigidbody2d;
        private Vector2 reducedVelocity;
        private bool hasAppliedSlowness = false;

        public void Apply(float duration, float interval, float intensity, bool refreshDuration, bool stackIntensity, bool logTicks)
        {
            this.logTicks = logTicks;
            rigidbody2d = GetComponent<Rigidbody2D>();

            if (refreshDuration || durationRemaining <= 0f)
            {
                durationRemaining = Mathf.Max(0.05f, duration);
            }

            if (!hasAppliedSlowness)
            {
                // Apply 30% speed reduction on first apply
                if (rigidbody2d != null)
                {
                    reducedVelocity = rigidbody2d.velocity * 0.7f; // 70% of original = 30% reduction
                    rigidbody2d.velocity = reducedVelocity; // TODO: Reduce Zombie Agent Speed
                    hasAppliedSlowness = true;

                    if (logTicks)
                    {
                        Debug.Log($"Water slow applied to {gameObject.name} (30% speed reduction)");
                    }
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
        }
    }
}
