using UnityEngine;
using UnityEngine.SceneManagement; // Required for Scene Handling

public class MainMenuController : MonoBehaviour
{

    // ?: Play Button Functionality, loads the Gameplay Scene
    //    *'sceneName' parameter is taken from the input found on the PlayButton itself!
    public void OnClick_PlayButton(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // ?: Quit Button Functionality, closes the game
    public void OnClick_QuitButton()
    {
        Application.Quit();
    }

}
