using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSelectionController : MonoBehaviour
{
    // Loads the Math Game scene
    public void LoadMathGame()
    {
        SceneManager.LoadScene("MathGame");
    }

    // Loads the Geography Game scene
    public void LoadGeographyGame()
    {
        SceneManager.LoadScene("GeographyGame");
    }

    // Loads the FlappyBird Game scene
    public void LoadFlappyGame()
    {
        SceneManager.LoadScene("FlappyGame");
    }
}
