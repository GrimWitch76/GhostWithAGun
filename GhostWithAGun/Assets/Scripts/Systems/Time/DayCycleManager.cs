using System;
using System.Collections;
using UnityEngine;

public enum DayCycleState
{
    Day,
    NightWaitingForSpawn,
    NightActive,
    SunriseTransition,
    GameComplete
}

public class DayCycleManager : MonoBehaviour
{
    public static DayCycleManager Instance { get; private set; }

    [Header("Config / Sequence")]
    public NightConfig[] _nightConfigs;
    [Tooltip("Start night index (0-based).")]
    public int startingNightIndex = 0;

    [Header("Ghost")]
    public GameObject ghostPrefab;
    public Transform[] ghostSpawnPoints;
    public string ghostTag = "Ghost"; // used when cleaning up
    private GameObject activeGhost;

    [Header("Transitions / UX")]
    public float fadeOutDuration = 1.0f;
    public float surviveMessageDuration = 2.0f;
    public float fadeInDuration = 1.0f;

    [Tooltip("Where the player wakes up between nights.")]
    public Transform gunRoomWakePoint;
    public CanvasGroup blackoutCanvas;     // simple full-screen canvas group
    public TMPro.TMP_Text surviveText;     // optional “You Survived” text

    [Header("Hooks (Optional)")]
    public AudioSource musicSource;
    public Action<int> OnNightAdvanced;       // called with nextNightIndex
    public Action<int> OnGhostSpawned;        // called with currentNightIndex
    public Action<int> OnNightSurvived;       // called with currentNightIndex
    public Action OnAllNightsComplete;

    public DayCycleState State { get; private set; }
    public int CurrentNightIndex { get; private set; }

    private Coroutine spawnRoutine;
    private GameObject _player;
    private SaveManager _saveManager;
    private bool nightWasActiveThisCycle;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        TimeManager.OnDayStarted += HandleDayStarted;
        TimeManager.OnNightStarted += HandleNightStarted;
    }

    private void OnDisable()
    {
        TimeManager.OnDayStarted -= HandleDayStarted;
        TimeManager.OnNightStarted -= HandleNightStarted;
    }

    private void Start()
    {
        // Initialize progression
        CurrentNightIndex = Mathf.Clamp(startingNightIndex, 0, MaxNightIndex());
        State = DayCycleState.Day;

        // Ensure blackout is hidden initially
        if (blackoutCanvas) blackoutCanvas.alpha = 0f;
        if (surviveText) surviveText.enabled = false;
        HandleDayStarted();
    }

    private int MaxNightIndex() => _nightConfigs.Length;

    private NightConfig CurrentNight => _nightConfigs[CurrentNightIndex];

    private void HandleDayStarted()
    {
        // If we came from night, handle sunrise transition
        if (nightWasActiveThisCycle)
        {
            nightWasActiveThisCycle = false;
            if (State != DayCycleState.SunriseTransition && State != DayCycleState.GameComplete)
            {
                StartCoroutine(SunriseSequence());
            }
        }
        else
        {
            ApplyDayTimeOverride();
            // Normal daytime idle
            State = DayCycleState.Day;
            // Optional: day music / ambience here
        }
    }

    private void HandleNightStarted()
    {
        if (State == DayCycleState.GameComplete) return;
        State = DayCycleState.NightWaitingForSpawn;

        // Start the spawn timer
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        spawnRoutine = StartCoroutine(Co_SpawnGhostAfterDelay());
    }

    private void ApplyDayTimeOverride()
    {
        var cfg = CurrentNight;
        if (cfg != null && cfg.PauseDayTime && TimeManager.Instance.CurrentPeriod == TimePeriod.Day)
        {
            TimeManager.Instance.PauseTime(true);
        }
    }

    private IEnumerator Co_SpawnGhostAfterDelay()
    {
        var cfg = CurrentNight;
        float delay = (cfg != null) ? cfg.SpawnDelay : 15f;
        if (cfg != null && cfg.SpawnDelayVariance > 0f)
        {
            delay += UnityEngine.Random.Range(-cfg.SpawnDelayVariance, cfg.SpawnDelayVariance);
            delay = Mathf.Max(0f, delay);
        }

        float t = 0f;
        while (t < delay && State == DayCycleState.NightWaitingForSpawn)
        {
            // If time was skipped to day, bail
            if (TimeManager.Instance.CurrentPeriod == TimePeriod.Day) yield break;

            t += Time.deltaTime;
            yield return null;
        }

        if (State != DayCycleState.NightWaitingForSpawn ||
            TimeManager.Instance.CurrentPeriod != TimePeriod.Night)
            yield break;

        SpawnGhost();
    }

    private void SpawnGhost()
    {
        nightWasActiveThisCycle = true;
        State = DayCycleState.NightActive;

        if (!ghostPrefab) { Debug.LogWarning("No ghostPrefab assigned."); return; }

        Transform spawn = (ghostSpawnPoints != null && ghostSpawnPoints.Length > 0)
            ? ghostSpawnPoints[UnityEngine.Random.Range(0, ghostSpawnPoints.Length)]
            : null;

        activeGhost = Instantiate(ghostPrefab, spawn ? spawn.position : Vector3.zero, spawn ? spawn.rotation : Quaternion.identity);

        // Apply behavior config to ghost
        ApplyGhostConfig(activeGhost, CurrentNight);

        OnGhostSpawned?.Invoke(CurrentNightIndex);
    }

    private void ApplyGhostConfig(GameObject ghost, NightConfig cfg)
    {
    }

    private void DespawnGhost()
    {
        //if (activeGhost)
        //{
        //    Destroy(activeGhost);
        //    activeGhost = null;
        //}

        //// Also clean up any lingering ghosts by tag (failsafe)
        //if (!string.IsNullOrEmpty(ghostTag))
        //{
        //    var leftovers = GameObject.FindGameObjectsWithTag(ghostTag);
        //    foreach (var g in leftovers) Destroy(g);
        //}
    }

    // ---------- Sunrise (Survived) Sequence ----------

    private IEnumerator SunriseSequence()
    {
        State = DayCycleState.SunriseTransition;

        // Stop ghost & night content
        DespawnGhost();

        // Pause world time during transition (optional)
        TimeManager.Instance.PauseTime(true);

        // Fade to black
        yield return FadeCanvas(blackoutCanvas, 0f, 1f, fadeOutDuration);

        // Show “You survived Night N” message
        if (surviveText)
        {
            surviveText.enabled = true;
            surviveText.text = $"You Survived Night {CurrentNightIndex + 1}";
        }
        OnNightSurvived?.Invoke(CurrentNightIndex);

        yield return new WaitForSecondsRealtime(surviveMessageDuration);

        // Hide message
        if (surviveText) surviveText.enabled = false;

        // Teleport player to gun room / reset position
        if (gunRoomWakePoint)
        {
            if(_player == null) //cache for future use
            {
                _player = FindFirstObjectByType<CharacterController>().gameObject;
            }

            _player.transform.SetPositionAndRotation(gunRoomWakePoint.position, gunRoomWakePoint.rotation);
        }

        // Advance to next night
        if (CurrentNightIndex >= MaxNightIndex())
        {
            // All nights complete – wrap up
            OnAllNightsComplete?.Invoke();
            State = DayCycleState.GameComplete;
            // (Optional) Load an end scene, show credits, etc.
            // SceneManager.LoadScene("EndingScene");
        }
        else
        {
            CurrentNightIndex++;
            OnNightAdvanced?.Invoke(CurrentNightIndex);
        }

        // Resume time and fade back in to day
        TimeManager.Instance.PauseTime(false);
        yield return FadeCanvas(blackoutCanvas, 1f, 0f, fadeInDuration);

        State = DayCycleState.Day;

        // Optional: trigger save here
        if(_saveManager == null)
        {
            _saveManager = FindFirstObjectByType<SaveManager>();
        }
        _saveManager.SetCurrentNight(CurrentNightIndex);
        _saveManager.SaveNight();
    }

    // ---------- Utilities ----------

    private IEnumerator FadeCanvas(CanvasGroup cg, float from, float to, float duration)
    {
        if (!cg) yield break;
        float t = 0f;
        cg.blocksRaycasts = true;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        cg.alpha = to;
        cg.blocksRaycasts = (to > 0.99f);
    }

    // Call when player dies to short-circuit the loop (optional)
    public void OnPlayerDied()
    {
        // Stop ghost, show fail UI, etc.
        DespawnGhost();
        // Optionally pause time, show “You Died” and present retry/quit
        // This can be integrated with your existing GameManager
    }

}
