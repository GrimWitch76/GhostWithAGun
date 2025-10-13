using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void UI_LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
