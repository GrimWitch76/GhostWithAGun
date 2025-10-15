using UnityEngine;

public class LightSwitch : MonoBehaviour, IInteractable
{
    [Header("Lightswitch Settings")]
    [SerializeField] private Light[] _targetLights;     // Lights to toggle
    [SerializeField] private bool _startOn = true;
    [SerializeField] private Transform _lever;          // The switch lever or toggle mesh
    [SerializeField] private float _offAngle = 0f;
    [SerializeField] private float _onAngle = 15f;
    [SerializeField] private float _switchSpeed = 8f;   // Speed of the lever moving
    [SerializeField] private SoundEmitter _soundEmitter;

    private bool _isOn;
    private float _currentAngle;
    private float _targetAngle;

    private void Awake()
    {
        _isOn = _startOn;
        _targetAngle = _isOn ? _onAngle : _offAngle;
        _currentAngle = _targetAngle;
        ToggleLights(_isOn);

        // Snap lever to correct starting rotation
        if (_lever != null)
        {
            Vector3 euler = _lever.localEulerAngles;
            euler.x = _targetAngle;
            _lever.localEulerAngles = euler;
        }
    }

    public void Interact(PlayerInteraction interactor)
    {
        if (_targetLights.Length == 0) return;

        _isOn = !_isOn;
        _soundEmitter?.PlayOnce();
        ToggleLights(_isOn);

        _targetAngle = _isOn ? _onAngle : _offAngle;
    }

    public void GhostInteract(GameObject interactor) { }

    private void Update()
    {
        // Smoothly rotate the lever toward the target angle
        if (_lever != null)
        {
            _currentAngle = Mathf.Lerp(_currentAngle, _targetAngle, Time.deltaTime * _switchSpeed);
            Vector3 euler = _lever.localEulerAngles;
            euler.x = _currentAngle;
            _lever.localEulerAngles = euler;
        }
    }

    private void ToggleLights(bool enabled)
    {
        foreach (var light in _targetLights)
        {
            if (light != null)
                light.enabled = enabled;
        }
    }

    public void SetState(bool state)
    {
        _isOn = state;
        ToggleLights(_isOn);
        _targetAngle = _isOn ? _onAngle : _offAngle;
    }

    public bool GetState() => _isOn;
}
