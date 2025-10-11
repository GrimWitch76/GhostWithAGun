using UnityEngine;

public class Clock : MonoBehaviour
{
    [SerializeField] private Transform _hourHand, _minuteHand;

    [Header("Settings")]
    [SerializeField] private float _hoursToDegrees = 30f;  // 360° / 12
    [SerializeField] private float _minutesToDegrees = 6f; // 360° / 60

    [SerializeField] private float _rotationOffset = 90f; // Positive = clockwise correction

    private int lastHour = -1;

    private void OnEnable()
    {
        TimeManager.OnTimeUpdated += UpdateClock;
    }

    private void OnDisable()
    {
        TimeManager.OnTimeUpdated -= UpdateClock;
    }

    private void UpdateClock(float normalized)
    {
        TimeManager.Instance.GetCurrentGameTime(out int hour, out int minute);

        // Calculate angles
        float hourAngle = -((hour % 12) * _hoursToDegrees + (minute / 60f) * _hoursToDegrees);
        float minuteAngle = -(minute * _minutesToDegrees);

        // Apply rotation offset so 12 o'clock points upward
        hourAngle += _rotationOffset;
        minuteAngle += _rotationOffset;

        // Rotate hands
        if (_hourHand)
            _hourHand.localRotation = Quaternion.Euler(0f, hourAngle, 0f);
        if (_minuteHand)
            _minuteHand.localRotation = Quaternion.Euler(0f, minuteAngle, 0f);

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
        Debug.Log($"Chime! The time is now {hour}:00");
    }
}
