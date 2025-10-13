using Unity.Cinemachine;
using UnityEngine;

public class CameraSettings : MonoBehaviour
{
    [SerializeField] CinemachineInputAxisController _controller;


    private void Start()
    {
        
    }
    private void OnEnable()
    {
        SettingsItem.OnFloatSettingChanged += UpdateSetting;
    }

    private void OnDisable()
    {
        SettingsItem.OnFloatSettingChanged += UpdateSetting;
    }

    private void UpdateSetting(string name, float val)
    {
        if(name == "Sensitivity")
        {
            _controller.Controllers[0].Input.Gain = val / 100;
            _controller.Controllers[1].Input.Gain = -(val / 100);
        }
    }
}