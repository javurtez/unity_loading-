using UnityEngine;

public class AudioManager : MonoManager<AudioManager>
{
    public AudioClip clickAudioClip;

    public AudioSource audioSource;

    public void PlayClick()
    {
        audioSource.PlayOneShot(clickAudioClip);
    }
}