using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    void Awake()
    {
        // Singleton Setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public static void PlayGunfire(GunDefinition definition, GameObject source)
    {
        PlaySound(definition.fx.shootSound, source);
    }

    public static void PlayReload(GunDefinition definition, GameObject source)
    {
        PlaySound(definition.fx.reloadSound, source);
    }

    public static void PlaySound(Sound sound, GameObject source, AudioSource audioSource = null)
    {
        if (sound.clip == null)
        {
            return;
        }

        if (audioSource == null)
        {
            audioSource = source.AddComponent<AudioSource>();
        }

        var pitch = sound.randomPitchModifier > 0f ? sound.pitch + Random.Range(-sound.randomPitchModifier, sound.randomPitchModifier) : sound.pitch;
        audioSource.clip = sound.clip;
        audioSource.volume = sound.volume;
        audioSource.pitch = pitch;
        audioSource.loop = sound.isLoop;
        audioSource.spatialBlend = sound.spatialBlend;
        audioSource.minDistance = sound.minDistance;
        audioSource.maxDistance = sound.maxDistance;
        audioSource.Play();
    }
}