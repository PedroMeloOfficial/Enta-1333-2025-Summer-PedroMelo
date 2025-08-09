// SceneLoader.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static string TargetScene;

    public static void Load(string sceneName)
    {
        TargetScene = sceneName;
        SceneManager.LoadScene("LoadingScene"); // must match your Loading scene name
    }

    public static void Reload() => Load(SceneManager.GetActiveScene().name);
}
