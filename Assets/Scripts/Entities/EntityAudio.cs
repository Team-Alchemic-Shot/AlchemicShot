using UnityEngine;

    [RequireComponent(typeof(MovementBase))]
    [RequireComponent(typeof(Health))]
    public class EntityAudio : EntityComponent
    {
        [Header("Audio Profiles")]
        [Tooltip("Enable 'Random Pitch' to prevent repetitive footsetp noises.")]
        public Sound footstepSound;

        public Sound hurtSound;


    protected override void Awake()
    {
        if (!TryGetComponent<MovementBase>(out var movement))
        {
            Debug.LogWarning("EntityAudio requires a MovementBase component to function properly.");
            return;
        }

        if (!TryGetComponent<Health>(out var health))
        {
            Debug.LogWarning("EntityAudio requires a HealthBase component to function properly.");
            return;
        }

        movement.OnWalk += PlayFootstep;
        movement.OnRun += PlayFootstep;
        health.OnDamaged += PlayHurtSound;
    }

    public void PlayFootstep()
        {
            if (footstepSound != null && footstepSound.clip != null)
            {
                SoundManager.PlaySound(footstepSound, Entity.gameObject);
            }
        } 

        public void PlayHurtSound(Health a)
        {
            if (hurtSound != null && hurtSound.clip != null)
            {
                SoundManager.PlaySound(hurtSound, Entity.gameObject);
            }
        }
    }
