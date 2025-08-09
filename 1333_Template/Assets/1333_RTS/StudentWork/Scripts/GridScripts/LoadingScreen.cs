// LoadingScreen.cs
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Slider progressBar;           // optional
    [SerializeField] private float minimumShowTime = 0.8f; // prevents flash

    private void Start()
    {
        if (string.IsNullOrEmpty(SceneLoader.TargetScene))
        {
            SceneManager.LoadScene("MainMenu"); // fallback
            return;
        }
        StartCoroutine(LoadRoutine(SceneLoader.TargetScene));
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        float shown = 0f;

        // start async load but hold activation
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            shown += Time.unscaledDeltaTime;
            UpdateUI(op.progress / 0.9f);
            yield return null;
        }

        // fully loaded, wait a beat if needed
        while (shown < minimumShowTime)
        {
            shown += Time.unscaledDeltaTime;
            UpdateUI(1f);
            yield return null;
        }

        UpdateUI(1f);
        op.allowSceneActivation = true;
    }

    private void UpdateUI(float t)
    {
        if (progressBar) progressBar.value = t;
    }
}