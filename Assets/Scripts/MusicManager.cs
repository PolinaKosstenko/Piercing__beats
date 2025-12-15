using System.Collections.Generic;
using UnityEngine;

public enum Songs
{
    DyingStar = 0,
    Welcome,
    LastDestination
}

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField] private List<AudioClip> backgroundMusic;
    AudioClip current;
    private AudioSource audioSource;

    public void SwitchSong(Songs s)
    {
        current = backgroundMusic[(int)s];
    }

    void Awake()
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

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        current = backgroundMusic[(int)Songs.DyingStar];
        audioSource.clip = current;
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        audioSource.volume = 0.15f;
    }

    void Start()
    {
        PlayBackgroundMusic();
    }

    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null && audioSource != null)
        {
            audioSource.clip = current;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}