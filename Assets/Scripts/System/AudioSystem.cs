/*--------------------------------------------------------------------------------*
  File Name: AudioSystem.cs
  Authors: Haneul Lee

  Copyright DigiPen Institute of Technology
 *--------------------------------------------------------------------------------*/

using UnityEngine;

public class AudioSystem : MonoBehaviour
{
    public static AudioSystem Instance;

    public AudioSource musicSource;
    public AudioClip[] musicClips; 

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySFXAtPosition(AudioClip clip, Vector3 position)
    {
        AudioSource.PlayClipAtPoint(clip, position);
    }

    public void PlayMusic(int index, bool loop = true)
    {
        if (musicClips == null || musicClips.Length <= index)
        {
            Debug.LogWarning("Music index out of range!");
            return;
        }

        musicSource.clip = musicClips[index];
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource.isPlaying)
            musicSource.Stop();
    }

    public void PauseMusic()
    {
        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        musicSource.UnPause();
    }

    public void SetBGMVolume(float volume)
    {
        musicSource.volume = Mathf.Clamp01(volume);
    }
}