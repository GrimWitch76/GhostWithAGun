using System.Collections;
using UnityEngine;

public class FlashLight : MonoBehaviour
{
    [SerializeField] private bool _enableFlashLight;

    [Header("References")]
    [SerializeField] private Light _light;
    [SerializeField] private SoundEmitter _sound;

    [Header("Flicker Settings")]
    [SerializeField] private float flickerRate = 0.1f;       // Time between flicker checks
    [SerializeField] private float flickerStrength = 0.2f;   // How much the intensity varies
    [SerializeField] private bool _enableFlicker = true;     // Optional toggle for flickering

    private bool _isEnabled;
    private float _baseIntensity;
    private Coroutine _flickerRoutine;

    private void Awake()
    {
        _baseIntensity = _light.intensity;
        _light.enabled = false;
    }

    public void ToggleLight()
    {
        if(!_enableFlashLight)
        {
            return;
        }
         
        _isEnabled = !_isEnabled;
        _light.enabled = _isEnabled;

        _sound?.PlayOnce();

        if (_isEnabled && _enableFlicker)
        {
            if (_flickerRoutine == null)
                _flickerRoutine = StartCoroutine(Flicker());
        }
        else if (!_isEnabled && _flickerRoutine != null)
        {
            StopCoroutine(_flickerRoutine);
            _flickerRoutine = null;
        }
    }

    private IEnumerator Flicker()
    {
        while (_isEnabled)
        {
            // Random chance to flicker slightly or fully
            float flickerChance = Random.value;

            if (flickerChance < 0.05f)
            {
                // Full "blink off" flicker
                _light.enabled = false;
                yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
                _light.enabled = true;
            }
            else
            {
                // Small intensity variation
                float randomOffset = Random.Range(-flickerStrength, flickerStrength);
                _light.intensity = Mathf.Clamp(_baseIntensity + randomOffset, 0f, _baseIntensity + flickerStrength);
            }

            yield return new WaitForSeconds(flickerRate);
        }

        // Reset when turned off
        _light.intensity = _baseIntensity;
    }
}
