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
        if (context.Target == null)
        {
            return;
        }

        var tag = ElementTag.GetOrAddTag<WaterDOTTag>(context.Target);
        TrackTag(tag, context);

        tag.Apply(duration, tickInterval, defaultIntensity, refreshDuration, stackIntensity, context.instigator, logTicks);
    }

    public override void RevertEffects(ElementBehaviorContext context)
    {
        if (context.Target == null)
        {
            return;
        }

        if (context.Target.TryGetComponent<WaterDOTTag>(out var tag)
             && context.Target.TryGetComponent<ZombieSpeedController>(out var speed))
        {
            speed.ClearSpeedMultiplier(tag);
            tag.ResetAppliedSpeedMultiplier();
        }
    }

    public class WaterDOTTag : ElementTag
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
            if (!TryGetComponent<ZombieSpeedController>(out var speed))
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

           
                
            //  update slow multiplier via ZombieSpeedController

            var desiredMultiplier = 1f / intensity;

            // clamp 
            desiredMultiplier = Mathf.Clamp(desiredMultiplier, 0f, 1f);

            appliedSpeedMultiplier = desiredMultiplier;

            
            //  recompute final speed every frame.
            speed.SetSpeedMultiplier(this, appliedSpeedMultiplier);

            hasAppliedSlowness = true;
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
