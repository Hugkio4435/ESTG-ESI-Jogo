using UnityEngine;
using UnityEngine.Audio;
using System;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
}

public class AudioManager : MonoBehaviour
{
    
    public static AudioManager Instance;

    [Header("Configuração de Áudio")]
    public AudioMixerGroup musicMixerGroup;
    public AudioMixerGroup sfxMixerGroup;

    [Header("As Tuas Bibliotecas de Som")]
    public Sound[] musicSounds;
    public Sound[] sfxSounds;

   
    private AudioSource musicSource;
    private AudioSource sfxSource;

    private void Awake()
    {
        
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

        
        musicSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();

        
        musicSource.outputAudioMixerGroup = musicMixerGroup;
        sfxSource.outputAudioMixerGroup = sfxMixerGroup;

        
        musicSource.loop = true;
    }

    public void PlayMusic(string soundName)
    {
        Sound s = Array.Find(musicSounds, x => x.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("Música não encontrada: " + soundName);
            return;
        }


        if (musicSource.clip == s.clip && musicSource.isPlaying)
        {
            return; 
        }

        musicSource.clip = s.clip;
        musicSource.volume = s.volume;
        musicSource.pitch = s.pitch;
        musicSource.Play();
    }

    public void PlaySFX(string soundName)
    {
        Sound s = Array.Find(sfxSounds, x => x.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("SFX não encontrado: " + soundName);
            return;
        }

        sfxSource.PlayOneShot(s.clip, s.volume);
    }
}