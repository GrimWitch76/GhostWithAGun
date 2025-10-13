using System;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class SettingsItem : MonoBehaviour
{
    [Header("Setting Config")]
    [SerializeField] private string _settingsName;              // e.g. "MasterVolume"
    [SerializeField] private SettingType _settingType;
    [SerializeField] private TextMeshProUGUI _sliderText;

    [Header("Optional References")]
    [SerializeField] private Slider _slider;
    [SerializeField] private Toggle _toggle;
    [SerializeField] private AudioMixer _audioMixer;            // Assign your main mixer here
    [SerializeField] private Volume _postProcessVolume;         // For gamma/exposure changes

    private ColorAdjustments _colorAdjustments;

    public static event Action<string, float> OnFloatSettingChanged;
    public static event Action<string, bool> OnBoolSettingChanged;

    private enum SettingType
    {
        MasterVolume,
        MusicVolume,
        SFXVolume,
        Gamma,
        Fullscreen,
        VSync,
        Sensitivity
    }

    private void Start()
    {
        if (_slider != null)
        {
            float savedValue = PlayerPrefs.GetFloat(_settingsName, GetDefaultValue());
            _slider.value = savedValue;
            UpdateSliderUI(savedValue);
            ApplySetting(savedValue);
        }

        if (_toggle != null)
        {
            bool savedBool = PlayerPrefs.GetInt(_settingsName, 1) == 1;
            _toggle.isOn = savedBool;
            ApplySetting(savedBool);
        }
    }

    private float GetDefaultValue()
    {
        return _settingType == SettingType.Gamma ? 1f : 100f;
    }

    public void UpdateSettingValue(float value)
    {
        PlayerPrefs.SetFloat(_settingsName, value);
        UpdateSliderUI(value);
        ApplySetting(value);
        OnFloatSettingChanged?.Invoke(_settingsName, value);
    }

    public void UpdateSettingValue(bool value)
    {
        PlayerPrefs.SetInt(_settingsName, value ? 1 : 0);
        ApplySetting(value);
        OnBoolSettingChanged?.Invoke(_settingsName, value);
    }

    private void UpdateSliderUI(float value)
    {
        if (_sliderText != null)
        {
            _sliderText.text = Mathf.RoundToInt(value).ToString();
        }
    }

    private void ApplySetting(float value)
    {
        switch (_settingType)
        {
            case SettingType.MasterVolume:
                SetVolume("MasterVolume", value);
                break;
            case SettingType.MusicVolume:
                SetVolume("Music", value);
                break;
            case SettingType.SFXVolume:
                SetVolume("SFX", value);
                break;
        }
    }

    private void ApplySetting(bool value)
    {
        switch (_settingType)
        {
            case SettingType.Fullscreen:
                Screen.fullScreen = value;
                break;
            case SettingType.VSync:
                QualitySettings.vSyncCount = value ? 1 : 0;
                break;
        }
    }

    private void SetVolume(string parameterName, float value)
    {
        if (_audioMixer == null) return;

        // Convert 0–100 slider to decibels (-80 dB to 0 dB)
        float dB = Mathf.Lerp(-80f, 0f, value / 100f);
        _audioMixer.SetFloat(parameterName, dB);
        float val;
        _audioMixer.GetFloat(parameterName, out val);
        Debug.Log(val);
    }
}
