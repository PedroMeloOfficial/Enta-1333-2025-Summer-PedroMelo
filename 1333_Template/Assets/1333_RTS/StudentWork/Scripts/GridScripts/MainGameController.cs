using UnityEngine;

public class MainGameController : MonoBehaviour
{
    [SerializeField] private GridManager manager;
    private bool musicMuted = false;
    private bool sfxMuted = false;

    public Pathfinding NavGrid { get; private set; }

    private void Awake()
    {
        AudioManager.Instance.PlayGameplayMusic();
        NavGrid = FindObjectOfType<Pathfinding>();
        if (!manager.IsInitialized)
        {
            manager.InitializeGrid();
        }

        if (NavGrid != null)
        {
            NavGrid.Initialise(manager);
        }
    }
    
    public void ToggleMusicMute()
    {
        musicMuted = !musicMuted;
        AudioManager.Instance.MuteMusic(musicMuted);
    }

    public void ToggleSFXMute()
    {
        sfxMuted = !sfxMuted;
        AudioManager.Instance.MuteSFX(sfxMuted);
    }
}