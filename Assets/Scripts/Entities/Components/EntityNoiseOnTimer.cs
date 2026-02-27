using UnityEngine;

public class EntityNoiseOnTimer : EntityComponent
{
    [Header("Noise Settings")]
    public Sound sound;
    public float noiseInterval = 7f; // Time in seconds between noises

    private float noiseTimer;

    protected override void Awake()
    {
        base.Awake();
        noiseTimer = noiseInterval; // Start the timer at the interval so it plays immediately
    }

    private void Update()
    {
        if (sound == null || sound.clip == null)
            return;

        noiseTimer -= Time.deltaTime;
        if (noiseTimer <= 0f)
        {
            SoundManager.PlaySound(sound, Entity.gameObject);
            noiseTimer = noiseInterval; // Reset the timer
        }
    }
}