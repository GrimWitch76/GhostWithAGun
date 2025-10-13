using TMPro;
using UnityEngine;

public class RandomTip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _tipField;
    [SerializeField] private string[] _tips;

    private void Awake()
    {
        if(_tipField != null && _tips.Length > 0)
        {
            _tipField.text = _tips[Random.Range(0, _tips.Length - 1)];
        }
    }
}
