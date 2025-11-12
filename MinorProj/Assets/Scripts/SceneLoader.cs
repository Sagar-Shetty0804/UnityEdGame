using UnityEngine;
using UnityEngine.SceneManagement; // <-- IMPORTANT! You must add this line.

public class SceneLoader : MonoBehaviour
{
    // This is the public function we will call from the buttons.
    // It takes one parameter: the string name of the scene to load.
    public void LoadGameScene(string sceneName)
    {
        // Make sure the sceneName is not empty
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("Scene name to load is not set on the button!");
        }
    }
}
