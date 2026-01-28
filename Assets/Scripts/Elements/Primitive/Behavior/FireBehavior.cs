using UnityEngine;

[CreateAssetMenu(fileName = "FireBehavior", menuName = "Elements/Behaviors/Fire")]
public class FireBehavior : ElementBehavior
{
    [SerializeField]
    private float duration = 3f;
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

        if (!context.target.TryGetComponent<FireDotStatus>(out var status))
        {
            status = context.target.AddComponent<FireDotStatus>();
        }

        float intensity = context.intensity <= 0f ? 1f : context.intensity;
        var damage = new DamageInfo(intensity, context.instigator, context.target.transform.position);
        status.Apply(
            duration, 
            tickInterval, 
            damage.amount, 
            refreshDuration, 
            stackIntensity, 
            context.instigator, 
            logTicks);
    }

    public class FireDotStatus : MonoBehaviour
    {
        private float durationRemaining;
        private float tickInterval;
        private float tickTimer;
        private float intensity;
        private bool logTicks;
        private GameObject instigator;

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
            this.instigator = instigator;
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
                if (TryGetComponent<Health>(out var health))
                {
                    health.ApplyDamage(new DamageInfo(intensity, instigator, transform.position));
                }
                if (logTicks)
                {
                    Debug.Log($"Fire DOT tick on {gameObject.name} (intensity: {intensity:F2})");
                }
            }
        }
    }
}
