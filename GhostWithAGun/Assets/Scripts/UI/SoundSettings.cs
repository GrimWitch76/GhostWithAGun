using UnityEngine;
using UnityEngine.Audio;
using static UnityEngine.Rendering.DebugUI;

public class SoundSettings : MonoBehaviour
{
    [SerializeField] AudioMixer _mixer;
    private void Start()
    {
        var sfx = PlayerPrefs.GetFloat("SFX", 1);
        var music = PlayerPrefs.GetFloat("Music", 1);
        float dB = Mathf.Lerp(-80f, 0f, sfx / 100f);
        _mixer.SetFloat("SFX", dB);

        dB = Mathf.Lerp(-80f, 0f, music / 100f);
        _mixer.SetFloat("Music", dB);
    }

    private void OnEnable()
    {
        SettingsItem.OnFloatSettingChanged += UpdateSettings;
    }

    private void OnDisable()
    {
        SettingsItem.OnFloatSettingChanged -= UpdateSettings;
    }


    private void UpdateSettings(string name, float value)
    {
        var music = PlayerPrefs.GetFloat(name, 1);
        float dB = Mathf.Lerp(-80f, 0f, value / 100f);
        _mixer.SetFloat(name, dB);
    }
}
