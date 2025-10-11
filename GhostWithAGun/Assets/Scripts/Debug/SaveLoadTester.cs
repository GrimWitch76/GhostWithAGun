using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveLoadTester : MonoBehaviour
{
    [SerializeField] private SaveManager saveManager;

    private void ReloadWithSave()
    {
        // simple single-scene reload test
        string scene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(scene);
        // after reload, SaveManager.Awake() will automatically read PlayerPrefs if you call LoadNight in Start()
    }

    public void SaveGame()
    {
        saveManager.SaveNight();
    }

    public void LoadGame()
    {
        ReloadWithSave();
    }

    public void ClearSave()
    {
        SaveStorage.Clear("Night_1");
    }
}