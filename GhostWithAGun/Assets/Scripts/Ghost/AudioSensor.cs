using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class AudioSensor : MonoBehaviour
{
    private GhostBrain _brain;

    private void Start()
    {
        _brain = GetComponent<GhostBrain>();
    }

    private void OnEnable()
    {
        SoundManager.OnSoundEmitted += HandleSound;
    }

    private void OnDisable()
    {
        SoundManager.OnSoundEmitted -= HandleSound;
    }

    private void HandleSound(SoundEvent sound)
    {
        // Example: weight sound by loudness + importance
        float weight = sound.Loudness * sound.Importance;

        // Maybe check distance too
        float distance = Vector3.Distance(transform.position, sound.Position);
        if (distance > sound.Loudness)
        {
            return;
        }
        float effectiveStrength = weight / Mathf.Max(distance, 1f);

        Debug.Log($"Ghost heard sound from {sound.Source.name} with strength {effectiveStrength}");

        // Decide what to do:
        // - Move toward it
        // - Update suspicion meter
        // - Ignore if too weak
        _brain.OnHeardSound(sound, effectiveStrength);

    }

}
