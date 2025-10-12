using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class SettingsItem : MonoBehaviour
{
    [SerializeField] private string _settingsName;
    [SerializeField] private TextMeshProUGUI _sliderText;

    [SerializeField] private Slider _slider;
    [SerializeField] private Toggle _toggle;

    private void Start()
    {
        if (_sliderText != null)
        {
            _sliderText.text = PlayerPrefs.GetInt(_settingsName, 100).ToString();
        }

        if (_toggle != null)
        {
            
            bool newValue = PlayerPrefs.GetInt(_settingsName, 1) == 1 ? true : false;
            _toggle.isOn = newValue;
        }

        if(_slider != null)
        {
            _slider.value = PlayerPrefs.GetInt(_settingsName, 100);
        }
    }

    public void UpdateSettingValue(float value)
    {
        _sliderText.text = value.ToString();
        PlayerPrefs.SetInt(_settingsName, (int)value);
    }

    public void UpdateSettingsValue(bool value)
    {
        int newValue = value ? 1 : 0;
        PlayerPrefs.SetInt(_settingsName, newValue);
    }
}
