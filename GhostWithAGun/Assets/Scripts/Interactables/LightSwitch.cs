using UnityEngine;

public class LightSwitch : MonoBehaviour, IInteractable
{
    [SerializeField] private Light[] _targetLights; // Assign in inspector
    [SerializeField] private bool _startOn = true;

    [SerializeField] private SoundEmitter _soundEmitter; //used to broadcast sound event for ghost and play actual audio that the player hears

    private bool isOn;

    private void Awake()
    {
        isOn = _startOn;
        ToggleLights(isOn);
    }

    public void Interact(PlayerInteraction interactor, float heldValue)
    {
        if (_targetLights.Length == 0) return;

        _soundEmitter?.MakeNoise();
        isOn = !isOn;
        ToggleLights(isOn);
    }

    private void ToggleLights(bool enabled)
    {
        foreach (var light in _targetLights)
        {
            light.enabled = isOn;
        }
    }

    public void GhostInteract(GhostInteraction interactor)
    {
        throw new System.NotImplementedException();
    }
}
