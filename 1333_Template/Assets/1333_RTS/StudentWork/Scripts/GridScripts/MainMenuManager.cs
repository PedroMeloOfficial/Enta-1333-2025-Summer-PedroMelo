using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public void OnPlayButtonPressed()
    {
        SceneLoader.Load("GamePlayScene"); // or whatever your game scene is called
    }
}