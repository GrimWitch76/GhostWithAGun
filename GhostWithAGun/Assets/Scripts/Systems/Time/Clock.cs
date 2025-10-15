using UnityEngine;

public class Clock : MonoBehaviour
{
    [SerializeField] private Transform _hourHand, _minuteHand;
    [SerializeField] private Transform _pendulum; // <–– add your pendulum transform here

    [Header("Clock Settings")]
    [SerializeField] private float _hoursToDegrees = 30f;  // 360° / 12
    [SerializeField] private float _minutesToDegrees = 6f; // 360° / 60
    [SerializeField] private float _rotationOffset = 90f;  // Positive = clockwise correction

    [Header("Pendulum Settings")]
    [SerializeField] private float _swingAmplitude = 25f;  // degrees from center
    [SerializeField] private float _swingSpeed = 1f;       // swings per second (1 = once per sec)
    [SerializeField] private bool _useDeltaTime = true;    // toggle for smooth or fixed swing rate

    private int lastHour = -1;
    private float _timeAccumulator;

    private void OnEnable()
    {
        TimeManager.OnTimeUpdated += UpdateClock;
    }

    private void OnDisable()
    {
        TimeManager.OnTimeUpdated -= UpdateClock;
    }

    private void Update()
    {
        // --- Pendulum Swing ---
        if (_pendulum != null)
        {
            // Increment time (you can tie this to unscaled time if desired)
            _timeAccumulator += (_useDeltaTime ? Time.deltaTime : Time.fixedDeltaTime) * _swingSpeed * Mathf.PI * 2f;

            // Calculate smooth back-and-forth rotation using sine wave
            float angle = Mathf.Sin(_timeAccumulator) * _swingAmplitude;

            // Apply rotation (assuming pivoted around Z)
            _pendulum.localRotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private void UpdateClock(float normalized)
    {
        TimeManager.Instance.GetCurrentGameTime(out int hour, out int minute);

        // Calculate angles
        float hourAngle = ((hour % 12) * _hoursToDegrees + (minute / 60f) * _hoursToDegrees);
        float minuteAngle = (minute * _minutesToDegrees);

        // Apply rotation offset so 12 o'clock points upward
        hourAngle += _rotationOffset;
        minuteAngle += _rotationOffset;

        // Rotate hands
        if (_hourHand)
            _hourHand.localRotation = Quaternion.Euler(0f, 0f, hourAngle);
        if (_minuteHand)
            _minuteHand.localRotation = Quaternion.Euler(0f, 0f, minuteAngle);

        // Optional chime
        if (hour != lastHour)
        {
            lastHour = hour;
            PlayChime(hour);
        }
    }

    private void PlayChime(int hour)
    {
        // TODO: trigger audio or animation
    }
}
