using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    [SerializeField] private int initialPoolSize = 2;
    [SerializeField] private int maxSourcesPerObject = 8;
    [SerializeField] private bool allowPoolGrowth = true;

    private readonly Dictionary<GameObject, List<AudioSource>> sourcePools = new();
    private readonly Dictionary<AudioSource, Dictionary<AudioClip, float>> oneShotEndTimes = new();

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
    
    /// <summary>
    /// Plays the given sound from the specified source GameObject. 
    /// If an AudioSource is provided, it will be used; otherwise, 
    /// an available AudioSource from the pool will be used or created if necessary. 
    /// The playOverwrite flag determines whether to stop any currently playing instance
    /// of the same clip before playing the new sound. 
    /// This method handles both one-shot and looping sounds, 
    /// as well as random pitch variation if enabled in the Sound definition.
    /// </summary>
    /// <param name="sound"></param>
    /// <param name="source"></param>
    /// <param name="audioSource"></param>
    /// <param name="playOverwrite">Whether to stop any currently playing instance of the same clip before playing the new sound</param>
    public static void PlaySound(Sound sound, GameObject source, AudioSource audioSource = null, bool playOverwrite = true)
    {
        if (Instance == null)
        {
            return;
        }

        if (sound.clip == null)
        {
            return;
        }

        var pool = Instance.GetPool(source);
        if (!playOverwrite && Instance.IsClipPlaying(pool, sound.clip))
        {
            return;
        }

        if (playOverwrite)
        {
            Instance.StopClipInPool(pool, sound.clip);
        }

        var chosenSource = audioSource != null ? audioSource : Instance.GetAvailableSource(pool, playOverwrite);
        if (chosenSource == null)
        {
            return;
        }

        var pitch = sound.enableRandomPitch ? sound.pitch + Random.Range(-sound.randomPitchModifier, sound.randomPitchModifier) : sound.pitch;
        chosenSource.clip = sound.clip;
        chosenSource.volume = sound.volume;
        chosenSource.pitch = pitch;
        chosenSource.loop = sound.isLoop;
        chosenSource.spatialBlend = sound.spatialBlend;
        chosenSource.minDistance = sound.minDistance;
        chosenSource.maxDistance = sound.maxDistance;
        sound.source = chosenSource;

        if (sound.isLoop)
        {
            chosenSource.Play();
        }
        else
        {
            chosenSource.loop = false;
            chosenSource.PlayOneShot(sound.clip);
            Instance.SetOneShotEndTime(chosenSource, sound.clip, pitch);
        }
    }

    private List<AudioSource> GetPool(GameObject source)
    {
        if (!sourcePools.TryGetValue(source, out var pool))
        {
            pool = new List<AudioSource>(initialPoolSize);
            sourcePools[source] = pool;

            for (var i = 0; i < initialPoolSize; i++)
            {
                pool.Add(CreatePooledSource(source));
            }
        }

        return pool;
    }

    private AudioSource CreatePooledSource(GameObject source)
    {
        var created = source.AddComponent<AudioSource>();
        created.playOnAwake = false;
        return created;
    }

    private AudioSource GetAvailableSource(List<AudioSource> pool, bool playOverwrite)
    {
        for (var i = 0; i < pool.Count; i++)
        {
            var candidate = pool[i];
            if (!IsSourceBusy(candidate))
            {
                return candidate;
            }
        }

        if (allowPoolGrowth && pool.Count < maxSourcesPerObject)
        {
            var created = CreatePooledSource(pool[0].gameObject);
            pool.Add(created);
            return created;
        }

        if (playOverwrite && pool.Count > 0)
        {
            pool[0].Stop();
            return pool[0];
        }

        return null;
    }

    private bool IsSourceBusy(AudioSource source)
    {
        if (source.isPlaying)
        {
            return true;
        }

        return IsAnyOneShotPlaying(source);
    }

    private bool IsAnyOneShotPlaying(AudioSource source)
    {
        if (!oneShotEndTimes.TryGetValue(source, out var clipEndTimes))
        {
            return false;
        }

        var now = Time.time;
        var stillPlaying = false;
        var expired = new List<AudioClip>();
        foreach (var entry in clipEndTimes)
        {
            if (entry.Value <= now)
            {
                expired.Add(entry.Key);
            }
            else
            {
                stillPlaying = true;
            }
        }

        for (var i = 0; i < expired.Count; i++)
        {
            clipEndTimes.Remove(expired[i]);
        }

        if (clipEndTimes.Count == 0)
        {
            oneShotEndTimes.Remove(source);
        }

        return stillPlaying;
    }

    private bool IsClipPlaying(List<AudioSource> pool, AudioClip clip)
    {
        for (var i = 0; i < pool.Count; i++)
        {
            var candidate = pool[i];
            if (candidate.isPlaying && candidate.clip == clip)
            {
                return true;
            }

            if (IsOneShotPlaying(candidate, clip))
            {
                return true;
            }
        }

        return false;
    }

    private void StopClipInPool(List<AudioSource> pool, AudioClip clip)
    {
        for (var i = 0; i < pool.Count; i++)
        {
            var candidate = pool[i];
            if (candidate.isPlaying && candidate.clip == clip)
            {
                candidate.Stop();
            }

            ClearOneShot(candidate, clip);
        }
    }

    private bool IsOneShotPlaying(AudioSource audioSource, AudioClip clip)
    {
        if (!oneShotEndTimes.TryGetValue(audioSource, out var clipEndTimes))
        {
            return false;
        }

        if (!clipEndTimes.TryGetValue(clip, out var endTime))
        {
            return false;
        }

        if (endTime <= Time.time)
        {
            clipEndTimes.Remove(clip);
            if (clipEndTimes.Count == 0)
            {
                oneShotEndTimes.Remove(audioSource);
            }
            return false;
        }

        return true;
    }

    private void ClearOneShot(AudioSource audioSource, AudioClip clip)
    {
        if (!oneShotEndTimes.TryGetValue(audioSource, out var clipEndTimes))
        {
            return;
        }

        if (clipEndTimes.Remove(clip) && clipEndTimes.Count == 0)
        {
            oneShotEndTimes.Remove(audioSource);
        }
    }

    private void SetOneShotEndTime(AudioSource audioSource, AudioClip clip, float pitch)
    {
        if (!oneShotEndTimes.TryGetValue(audioSource, out var clipEndTimes))
        {
            clipEndTimes = new Dictionary<AudioClip, float>();
            oneShotEndTimes[audioSource] = clipEndTimes;
        }

        var absPitch = Mathf.Max(0.0001f, Mathf.Abs(pitch));
        clipEndTimes[clip] = Time.time + (clip.length / absPitch);
    }
}