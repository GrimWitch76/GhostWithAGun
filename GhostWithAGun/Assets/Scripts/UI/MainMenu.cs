using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string _gameplaySceneName = "Gameplay";
    [SerializeField] private string _loadingSceneName = "LoadingScreen";
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float fadeTime = 0.5f;

    private AsyncOperation loadingScreenOp;
    private AsyncOperation gameplayOp;



    public void UI_PlayGame()
    {
        StartCoroutine(LoadGameRoutine());
    }

    public void UI_ResetGame()
    {

    }

    public void UI_OpenReviews()
    {

    }

    public void UI_OpenSettings()
    {
        
    }

    private IEnumerator LoadGameRoutine()
    {
        // 1. Load the loading screen
        loadingScreenOp = SceneManager.LoadSceneAsync(_loadingSceneName, LoadSceneMode.Additive);
        while (!loadingScreenOp.isDone)
            yield return null;

        // 2. Optionally fade in / animate the loading UI

        var loadingScreen = UnityEngine.Object.FindAnyObjectByType<LoadingScreen>();
        yield return loadingScreen.FadeIn();
        loadingScreen.ShowScreen();
        yield return loadingScreen.FadeOut();

        // 3. Begin loading the gameplay scene
        gameplayOp = SceneManager.LoadSceneAsync(_gameplaySceneName, LoadSceneMode.Additive);
        gameplayOp.allowSceneActivation = false;

        // Wait for it to reach ~90%
        while (gameplayOp.progress < 0.9f)
            yield return null;

        // 4. Activate the gameplay scene
        gameplayOp.allowSceneActivation = true;

        while (!gameplayOp.isDone)
            yield return null;

        // 5. Set active scene so inputs & lighting go there
        Scene gameplayScene = SceneManager.GetSceneByName(_gameplaySceneName);
        SceneManager.SetActiveScene(gameplayScene);

        // 6. Run your save system
        yield return LoadSaveData();

        // 7. Fade out and unload the loading screen
        yield return loadingScreen.FadeIn();
        loadingScreen.HideScreen();
        HideMenu();
        yield return loadingScreen.FadeOut();

        SceneManager.UnloadSceneAsync(_loadingSceneName);

        // 8. Optionally unload menu itself
        SceneManager.UnloadSceneAsync("MainMenu");
    }

    private IEnumerator LoadSaveData()
    {
        // Give the scene one frame to settle
        yield return null;

        var saveManager = UnityEngine.Object.FindAnyObjectByType<SaveManager>();
        if (saveManager)
        {
            bool loaded = saveManager.LoadNight(1);
            if (loaded)
                Debug.Log("Loaded saved state successfully.");
            else
                Debug.Log("No save found, starting fresh.");
        }
        else
        {
            Debug.LogWarning("No SaveManager found in loaded scene!");
        }
    }

    private void HideMenu()
    {
        _canvasGroup.alpha = 0f;
    }
}
