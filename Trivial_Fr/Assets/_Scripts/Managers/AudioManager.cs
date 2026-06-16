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
    // O Padrão Singleton para acesso global
    public static AudioManager Instance;

    [Header("Configuração de Áudio")]
    public AudioMixerGroup musicMixerGroup;
    public AudioMixerGroup sfxMixerGroup;

    [Header("As Tuas Bibliotecas de Som")]
    public Sound[] musicSounds;
    public Sound[] sfxSounds;

    // Leitores de Áudio
    private AudioSource musicSource;
    private AudioSource sfxSource;

    private void Awake()
    {
        // Garante que só existe UM AudioManager no jogo todo
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

        // Criar os leitores de áudio invisíveis no próprio objeto
        musicSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();

        // Ligar os leitores à mesa de mistura
        musicSource.outputAudioMixerGroup = musicMixerGroup;
        sfxSource.outputAudioMixerGroup = sfxMixerGroup;

        // A música deve repetir (loop)
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


        // NOVO: Verifica se o clip já está no leitor e se já está a tocar
        if (musicSource.clip == s.clip && musicSource.isPlaying)
        {
            return; // Sai da função imediatamente e não reinicia a música!
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

        // PlayOneShot permite tocar vários SFX ao mesmo tempo sem se cortarem uns aos outros
        sfxSource.PlayOneShot(s.clip, s.volume);
    }
}