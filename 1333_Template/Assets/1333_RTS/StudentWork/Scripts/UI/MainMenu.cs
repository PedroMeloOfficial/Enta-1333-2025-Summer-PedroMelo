using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Tooltip("Name of the scene to load when starting the game")]
    public string gameSceneName = "TestScene";

    // Called by the Start button.
    public void OnStartButton()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("Game scene name is not set in MainMenuController.");
            return;
        }
        SceneManager.LoadScene(gameSceneName);
    }

    // Called by the Quit button.
    public void OnQuitButton()
    {
        Debug.Log("Quit button pressed. Exiting game.");
        Application.Quit();

#if UNITY_EDITOR
        // Stop play mode in the editor
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}