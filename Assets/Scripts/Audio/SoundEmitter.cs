using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    [SerializeField]
    private Sound sound;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlaySound()
    {
        SoundManager.PlaySound(sound, gameObject, audioSource);
    }
}