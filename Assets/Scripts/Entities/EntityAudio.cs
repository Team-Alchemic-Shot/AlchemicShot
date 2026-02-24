using UnityEngine;


    public class EntityAudio : EntityComponent
    {
        [Header("Audio Profiles")]
        [Tooltip("Enable 'Random Pitch' to prevent repetitive footsetp noises.")]
        public Sound footstepSound;

        public Sound hurtSound;

        public void PlayFootstep()
        {
            if (footstepSound != null && footstepSound.clip != null)
            {
                SoundManager.PlaySound(footstepSound, Entity.gameObject);
            }
        }

        public void PlayHurtSound()
        {
            if (hurtSound != null && hurtSound.clip != null)
            {
                SoundManager.PlaySound(hurtSound, Entity.gameObject);
            }
        }
    }
