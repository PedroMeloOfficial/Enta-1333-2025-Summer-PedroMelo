using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")]
    public AudioClip unitAttackClip;
    public AudioClip unitHitClip;
    public AudioClip buildingPlaceClip;
    public AudioClip buildingSelectClip;
    public AudioClip mainMusicClip;
    public AudioClip ambientClip;

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

    private void Start()
    {
        PlayMusic(mainMusicClip);
    }

    // MUSIC
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null)
            return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void MuteMusic(bool mute)
    {
        if (musicSource != null)
            musicSource.mute = mute;
    }

    // SFX
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void MuteSFX(bool mute)
    {
        if (sfxSource != null)
            sfxSource.mute = mute;
    }
    
    public void PlayGameplayMusic()
    {
        PlayMusic(ambientClip);
    }
    
    public void PlayMainMenuMusic()
    {
        PlayMusic(mainMusicClip);
    }
}