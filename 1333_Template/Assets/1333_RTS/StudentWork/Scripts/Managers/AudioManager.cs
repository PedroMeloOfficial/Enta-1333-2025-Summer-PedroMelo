using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    [Tooltip("AudioSource component that will play your soundtrack")]
    public AudioSource musicSource;

    [Tooltip("Default soundtrack to play on Start (optional)")]
    public AudioClip defaultMusic;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Ensure there's an AudioSource
            if (musicSource == null)
                musicSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
        musicSource.volume = 0.1f;
    }

    private void Start()
    {
        // Play default music immediately if assigned
        if (defaultMusic != null)
            PlayMusic(defaultMusic, loop: true);
    }

    // Plays the given clip on the music source.
    public void PlayMusic(AudioClip clip, bool loop = false)
    {
        if (clip == null) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    // Stops playback of the current music.
    public void StopMusic()
    {
        musicSource.Stop();
    }
}
