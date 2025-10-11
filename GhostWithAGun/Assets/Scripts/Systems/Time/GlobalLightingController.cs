using System.Collections;
using UnityEngine;

public class GlobalLightingController : MonoBehaviour
{
    [SerializeField] Light _globalLight;

    [Header("Day Settings")]
    [SerializeField] private Color _dayColour;
    [SerializeField] private float _dayIntensity;

    [Header("Night Settings")]
    [SerializeField] private Color _nightColour;
    [SerializeField] private float _nightIntensity;

    [Header("Transition Settings")]
    [Tooltip("How long the transition between day and night takes (seconds).")]
    [SerializeField] private float _transitionDuration = 5f;

    [Tooltip("If true, automatically set initial state based on TimeManager's current period.")]
    [SerializeField] private bool _matchInitialState = true;

    private Coroutine _transitionRoutine;

    private void OnEnable()
    {
        TimeManager.OnDayStarted += DayStarted;
        TimeManager.OnNightStarted += NightStarted;
    }

    private void OnDisable()
    {
        TimeManager.OnDayStarted -= DayStarted;
        TimeManager.OnNightStarted -= NightStarted;
    }

    private void Start()
    {
        if (_matchInitialState)
        {
            if (TimeManager.Instance.CurrentPeriod == TimePeriod.Day)
            {
                _globalLight.color = _dayColour;
                _globalLight.intensity = _dayIntensity;
            }
            else
            {
                _globalLight.color = _nightColour;
                _globalLight.intensity = _nightIntensity;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void DayStarted()
    {
        if (_transitionRoutine != null)
            StopCoroutine(_transitionRoutine);

        _transitionRoutine = StartCoroutine(TransitionTo(_dayColour, _dayIntensity));
    }


    private void NightStarted()
    {
        if (_transitionRoutine != null)
            StopCoroutine(_transitionRoutine);

        _transitionRoutine = StartCoroutine(TransitionTo(_nightColour, _nightIntensity));
    }

    private IEnumerator TransitionTo(Color targetColor, float targetIntensity)
    {
        Color startColor = _globalLight.color;
        float startIntensity = _globalLight.intensity;
        float elapsed = 0f;

        while (elapsed < _transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _transitionDuration);
            _globalLight.color = Color.Lerp(startColor, targetColor, t);
            _globalLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
            yield return null;
        }

        _globalLight.color = targetColor;
        _globalLight.intensity = targetIntensity;
        _transitionRoutine = null;
    }
}
