using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] private Sound[] sounds;
    private Dictionary<string, Sound> soundDictionary;

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

        // Initialize Dictionary for fast lookup
        soundDictionary = new Dictionary<string, Sound>();
        foreach (Sound s in sounds)
        {
            if (!soundDictionary.ContainsKey(s.name))
            {
                soundDictionary.Add(s.name, s);
            }
            else
            {
                Debug.LogWarning($"Duplicate sound name found: {s.name}");
            }
        }
    }

    /// <summary>
    /// Plays a sound at a specific 3D position. Creates a temporary GameObject.
    /// </summary>
    public void PlaySound3D(string soundName, Vector3 position)
    {
        if (!soundDictionary.ContainsKey(soundName))
        {
            Debug.LogWarning($"Sound: {soundName} not found!");
            return;
        }

        Sound s = soundDictionary[soundName];
        
        // 1. Create a temporary GameObject
        GameObject soundObj = new GameObject("TempAudio_" + soundName);
        soundObj.transform.position = position;

        // 2. Add and configure AudioSource
        AudioSource audioSource = soundObj.AddComponent<AudioSource>();
        audioSource.clip = s.clip;
        audioSource.volume = s.volume;
        audioSource.spatialBlend = s.spatialBlend; // 1.0 is fully 3D
        audioSource.minDistance = s.minDistance;
        audioSource.maxDistance = s.maxDistance;
        audioSource.rolloffMode = AudioRolloffMode.Linear; // or Logarithmic
        
        // 3. Handle Pitch
        if (s.enableRandomPitch)
        {
            audioSource.pitch = s.pitch * (1f + Random.Range(-s.randomPitchModifier, s.randomPitchModifier));
        }
        else
        {
            audioSource.pitch = s.pitch;
        }

        // 4. Play and Destroy
        audioSource.Play();
        
        // Destroy the object after the clip finishes
        Destroy(soundObj, s.clip.length + 0.1f);
    }

    /// <summary>
    /// Plays a 2D sound (UI, Music) that is not attached to a position.
    /// </summary>
    public void PlaySound2D(string soundName)
    {
        if (!soundDictionary.ContainsKey(soundName)) return;

        Sound s = soundDictionary[soundName];
        
        // For 2D sounds, we can create a temporary object parented to the manager
        // Or reuse a centralized AudioSource if overlap isn't an issue.
        // Here creates a temp object for consistency:
        GameObject soundObj = new GameObject("TempAudio2D_" + soundName);
        soundObj.transform.parent = this.transform;
        
        AudioSource audioSource = soundObj.AddComponent<AudioSource>();
        audioSource.clip = s.clip;
        audioSource.volume = s.volume;
        audioSource.pitch = s.pitch;
        audioSource.spatialBlend = 0f; // 2D Sound

        audioSource.Play();
        Destroy(soundObj, s.clip.length + 0.1f);
    }
}