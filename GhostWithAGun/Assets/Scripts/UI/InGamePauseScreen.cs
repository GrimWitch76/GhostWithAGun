using UnityEngine;

public class InGamePauseScreen : MonoBehaviour
{
    public void OnEnable()
    {
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnDisable()
    {
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UI_OnQuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        return;
#endif

        Application.Quit();
    }

    public void UI_OpenSettings()
    {
        gameObject.SetActive(true);
    }

    public void UI_CloseSettings()
    {
        gameObject.SetActive(false);
    }
}
