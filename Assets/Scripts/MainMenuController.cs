using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void Start()
    {
        Debug.Log("Main Menu Loaded");
    }

    // Called when the Play button is pressed
    public void OnPlayButton()
    {
        Debug.Log("Play button pressed");
        SceneManager.LoadScene("GameScene");
    }

    // Called when the Options button is pressed
    public void OnOptionsButton()
    {
        SceneManager.LoadScene("OptionsMenu");
    }

    // Called when the Quit button is pressed
    public void OnQuitButton()
    {
        Debug.Log("Quit button pressed");
        Application.Quit();
    }
}