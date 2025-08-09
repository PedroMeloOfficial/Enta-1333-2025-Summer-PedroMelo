using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameStateHandler : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject lossScreen;

    [Header("Other")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private WaveSpawner waveSpawner;

    private bool gameOver = false;

    private void Update()
    {
        if (gameOver) return;

        // Check for loss
        if (gridManager.FriendlyBase == null)
        {
            TriggerLoss();
        }
    }

    public void TriggerWin()
    {
        if (gameOver) return;
        gameOver = true;
        Time.timeScale = 0f;
        winScreen.SetActive(true);
    }

    public void TriggerLoss()
    {
        if (gameOver) return;
        gameOver = true;
        Time.timeScale = 0f;
        lossScreen.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneLoader.Reload();
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        AudioManager.Instance.PlayMainMenuMusic();
        SceneLoader.Load("MainMenu");
    }
}