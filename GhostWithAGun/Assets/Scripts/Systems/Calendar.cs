using TMPro;
using UnityEngine;

public class Calendar : MonoBehaviour
{
    [SerializeField] DayCycleManager dayCycleManager;
    [SerializeField] private TextMeshProUGUI _date;
    private int _currentDay;
    private void OnEnable()
    {
        dayCycleManager.OnDayStart += SetCalendarDate;
    }

    private void OnDisable()
    {
        dayCycleManager.OnDayStart -= SetCalendarDate;
    }

    private void SetCalendarDate(int date)
    {
        _currentDay = date+1;
        SetText();
    }

    private void SetText()
    {
        _date.text = _currentDay.ToString();
    }
}
