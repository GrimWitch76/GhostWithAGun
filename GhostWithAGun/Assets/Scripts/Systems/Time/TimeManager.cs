using System;
using TMPro;
using UnityEngine;

public enum TimePeriod
{
    Day,
    Night
}

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("Durations (Real Seconds)")]
    [SerializeField] private float dayDuration = 15f;
    [SerializeField] private float nightDuration = 30f;

    [Header("In-Game Time Ranges")]
    [Tooltip("Start and end hours of the day period (24-hour clock).")]
    [SerializeField][Range(0f, 24f)] private float dayStartHour = 12f;    // 12:00 PM
    [SerializeField][Range(0f, 24f)] private float dayEndHour = 0f;       // 12:00 AM (midnight)

    [Tooltip("Start and end hours of the night period (24-hour clock).")]
    [SerializeField][Range(0f, 24f)] private float nightStartHour = 0f;   // 12:00 AM
    [SerializeField][Range(0f, 24f)] private float nightEndHour = 6f;     // 6:00 AM

    [Header("Debug Controls")]
    [SerializeField] private bool enableDebugControls = false;
    [SerializeField][Range(0f, 1f)] public float debugTimeSlider = 0f;
    [SerializeField] private bool applyDebugTime = false;
    [SerializeField] private TextMeshProUGUI _debugText;

    private float currentTime;          // seconds in current phase
    private float currentPhaseDuration;
    private TimePeriod currentPeriod;
    private bool isPaused;

    public static event Action OnDayStarted;
    public static event Action OnNightStarted;
    public static event Action<float> OnTimeUpdated; // normalized 0–1 of current phase

    public TimePeriod CurrentPeriod => currentPeriod;
    public float CurrentNormalizedTime => Mathf.Clamp01(currentTime / currentPhaseDuration);
    public bool IsPaused => isPaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        SetPeriod(TimePeriod.Day);
    }

    private void Update()
    {
        if (isPaused) return;

        if (enableDebugControls && applyDebugTime)
        {
            currentTime = debugTimeSlider * currentPhaseDuration;
            applyDebugTime = false;
            UpdateTimeEvent();
            return;
        }

        currentTime += Time.deltaTime;
        if (currentTime >= currentPhaseDuration)
        {
            if (currentPeriod == TimePeriod.Day)
                SetPeriod(TimePeriod.Night);
            else
                SetPeriod(TimePeriod.Day);
        }

        if (_debugText != null)
        {
            int hour, minute;
            TimeManager.Instance.GetCurrentGameTime(out hour, out minute);

            _debugText.text = $"{hour:00}:{minute:00}";
        }
        UpdateTimeEvent();
    }

    private void UpdateTimeEvent()
    {
        OnTimeUpdated?.Invoke(CurrentNormalizedTime);
    }

    private void SetPeriod(TimePeriod newPeriod)
    {
        currentPeriod = newPeriod;
        currentTime = 0f;
        currentPhaseDuration = (newPeriod == TimePeriod.Day) ? dayDuration : nightDuration;

        if (newPeriod == TimePeriod.Day)
            OnDayStarted?.Invoke();
        else
            OnNightStarted?.Invoke();
    }

    // --- Public Controls ---
    public void PauseTime(bool pause) => isPaused = pause;
    public void JumpToNight() { if (currentPeriod != TimePeriod.Night) SetPeriod(TimePeriod.Night); }
    public void JumpToDay() { if (currentPeriod != TimePeriod.Day) SetPeriod(TimePeriod.Day); }
    public void SetTimeNormalized(float normalized)
    {
        normalized = Mathf.Clamp01(normalized);
        currentTime = normalized * currentPhaseDuration;
        UpdateTimeEvent();
    }

    // --- Time Conversion Helpers ---
    public void GetCurrentGameTime(out int hour, out int minute)
    {
        float normalized = CurrentNormalizedTime;
        GetGameTimeForPeriod(currentPeriod, normalized, out hour, out minute);
    }

    public void GetGameTimeForPeriod(TimePeriod period, float normalized, out int hour, out int minute)
    {
        normalized = Mathf.Clamp01(normalized);

        float start = (period == TimePeriod.Day) ? dayStartHour : nightStartHour;
        float end = (period == TimePeriod.Day) ? dayEndHour : nightEndHour;

        // Handle wrap-around (e.g., 12 ? 0 or 22 ? 4)
        float range = (end >= start) ? (end - start) : (24f - start + end);
        float hourF = (start + normalized * range) % 24f;
        hour = Mathf.FloorToInt(hourF);
        minute = Mathf.FloorToInt((hourF - hour) * 60f);
    }
}
