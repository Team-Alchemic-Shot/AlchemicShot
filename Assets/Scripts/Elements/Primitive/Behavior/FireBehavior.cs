using UnityEngine;

[CreateAssetMenu(fileName = "FireBehavior", menuName = "Elements/Behaviors/Primitive/Fire")]
public class FireBehavior : ElementBehavior
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

        if (!context.target.TryGetComponent<FireDOTTag>(out var tag))
        {
            tag = context.target.AddComponent<FireDOTTag>();
        }
        TrackTag(tag, context);

        tag.Apply(
            duration, 
            tickInterval, 
            defaultIntensity, 
            refreshDuration, 
            stackIntensity, 
            context.instigator, 
            logTicks);
    }

    public class FireDOTTag : ElementTag
    {
        private float durationRemaining;
        private float tickInterval;
        private float tickTimer;
        private bool logTicks;
        private GameObject instigator;

        public float Intensity;

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
                Intensity += intensity;
            }
            else
            {
                Intensity = intensity;
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
                RemoveOwners();
                return;
            }

            durationRemaining -= Time.deltaTime;
            tickTimer -= Time.deltaTime;

            if (tickTimer <= 0f)
            {
                tickTimer += tickInterval;
                if (TryGetComponent<Health>(out var health))
                {
                    health.ApplyDamage(new DamageInfo(Intensity, instigator, transform.position));
                }
                if (logTicks)
                {
                    Debug.Log($"Fire DOT tick on {gameObject.name} (intensity: {Intensity:F2})");
                }
            }
        }
    }
}
