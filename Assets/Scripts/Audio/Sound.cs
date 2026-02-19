using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;           // The name used to play the sound (e.g., "Explosion")
    public AudioClip clip;        // The actual audio file

    [Range(0f, 1f)]
    public float volume = 0.7f;
    
    [Range(0.1f, 3f)]
    public float pitch = 1f;

    [Tooltip("Randomize pitch slightly for variation (good for gunshots/footsteps)")]
    public bool enableRandomPitch = false;
    [Range(0f, 0.5f)]
    public float randomPitchModifier = 0.1f;

    public bool isLoop = false;
    
    [Header("3D Settings")]
    [Range(0f, 1f)]
    public float spatialBlend = 1f; // 0 = 2D (UI), 1 = 3D (World)
    public float minDistance = 1f;
    public float maxDistance = 20f;
    
    [HideInInspector]
    public AudioSource source; // Used mainly for music/looping sounds
}