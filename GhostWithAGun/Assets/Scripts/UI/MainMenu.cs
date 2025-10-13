using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string _gameplaySceneName = "Gameplay";
    [SerializeField] private string _loadingSceneName = "LoadingScreen";
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float fadeTime = 0.5f;

    [SerializeField] private GameObject _settings;
    [SerializeField] private GameObject _reviews;

    private AsyncOperation loadingScreenOp;
    private AsyncOperation gameplayOp;

    private void Start()
    {
        UI_CloseReviews();
        UI_CloseSettings();
    }

    public void UI_PlayGame()
    {
        StartCoroutine(LoadGameRoutine(false));
    }

    public void UI_ResetGame()
    {
        StartCoroutine(LoadGameRoutine(true));
    }

    public void UI_OpenReviews()
    {
        _reviews.SetActive(true);
    }

    public void UI_CloseReviews()
    {
        _reviews.SetActive(false);
    }

    public void UI_OpenSettings()
    {
        _settings.SetActive(true);
    }

    public void UI_CloseSettings()
    {
        _settings.SetActive(false);
    }

    private IEnumerator LoadGameRoutine(bool resetGame)
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
        if(resetGame)
        {
            yield return ResetSaveGame();
        }
        else
        {
            yield return LoadSaveData();
        }

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
            int currentNight = SaveStorage.GetCurrentNight();
            bool loaded = saveManager.LoadNight(currentNight);
            if (loaded)
            {
                FindFirstObjectByType<DayCycleManager>().InitalizeGame(currentNight);
                Debug.Log("Loaded saved state successfully.");
            }
            else
            {
                FindFirstObjectByType<DayCycleManager>().InitalizeGame(1);
                Debug.Log("No save found, starting fresh.");
            }
        }
        else
        {
            Debug.LogWarning("No SaveManager found in loaded scene!");
        }
    }

    private IEnumerator ResetSaveGame()
    {
        // Give the scene one frame to settle
        yield return null;
        var saveManager = UnityEngine.Object.FindAnyObjectByType<SaveManager>();
        if (saveManager)
        {
            SaveStorage.ClearAll();
            int currentNight = SaveStorage.GetCurrentNight();
            bool loaded = saveManager.LoadNight(currentNight);
            if (loaded)
                Debug.Log("Loaded saved state successfully.");
            else
                Debug.Log("No save found, starting fresh.");
        }
        else
        {
            Debug.LogWarning("No SaveManager found in loaded scene!");
        }

        FindFirstObjectByType<DayCycleManager>().InitalizeGame(1);
    }

    private void HideMenu()
    {
        _canvasGroup.alpha = 0f;
    }
}
