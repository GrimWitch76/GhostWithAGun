using PSX;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Rendering.DebugUI;

public class PostProcessingSettings : MonoBehaviour
{
    [SerializeField]
    private ColorAdjustments _colorAdjustments;

    [SerializeField] FogController _fogController;
    [SerializeField] DitheringController DitheringController;
    [SerializeField] PixelationController PixelationController;
    [SerializeField] Volume _volume;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_volume != null)
            _volume.profile.TryGet(out _colorAdjustments);


        var val = PlayerPrefs.GetFloat("Brightness", 0.5f);
        if (_colorAdjustments != null)
            _colorAdjustments.postExposure.value = Mathf.Lerp(-1f, 1f, val/100); // maps 0–1 to -1–1 exposure

        //bool savedBool = PlayerPrefs.GetInt("PostProcessing", 1) == 1;
        //_fogController.SetEnabled(savedBool);
        //DitheringController.SetEnabled(savedBool);
        //PixelationController.SetEnabled(savedBool);
    }

    private void OnEnable()
    {
        SettingsItem.OnFloatSettingChanged += SetBrightness;
        SettingsItem.OnBoolSettingChanged += SetToggle;
    }

    private void OnDisable()
    {
        SettingsItem.OnFloatSettingChanged -= SetBrightness;
        SettingsItem.OnBoolSettingChanged -= SetToggle;
    }

    void SetToggle(string name, bool value)
    {
        if(name == "PostProcessing")
        {
            //_fogController.SetEnabled(value);
            //DitheringController.SetEnabled(value);
            //PixelationController.SetEnabled(value);
        }
    }

    void SetBrightness(string name, float value)
    {
        if(name == "Brightness")
        {
            _colorAdjustments.postExposure.value = Mathf.Lerp(-1f, 1f, value / 100); // maps 0–1 to -1–1 exposure
        }
    }

}
