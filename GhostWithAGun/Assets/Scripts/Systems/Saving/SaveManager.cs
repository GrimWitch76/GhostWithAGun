using UnityEngine;

public class SaveManager : MonoBehaviour
{
    [SerializeField] private SaveCollector collector;

    public int CurrentNight { get; private set; } = 1; // drive this from your night flow

    private string Slot => $"Night_{CurrentNight}";

    private void Start()
    {
        // Attempt to load automatically when scene starts
        if (LoadNight(CurrentNight))
            Debug.Log("Loaded existing save for night " + CurrentNight);
        else
            Debug.Log("No save found, starting fresh");
    }

    public void SaveNight()
    {
        var env = collector.CaptureAll(CurrentNight);
        string json = JsonUtility.ToJson(env);
        SaveStorage.WriteJson(Slot, json);
        Debug.Log($"Saved night {CurrentNight}");
    }

    public bool LoadNight(int night)
    {
        CurrentNight = night;
        string json = SaveStorage.ReadJson(Slot);
        if (string.IsNullOrEmpty(json)) return false;

        var env = JsonUtility.FromJson<SaveEnvelope>(json);
        collector.ApplyAll(env);
        Debug.Log($"Loaded night {night}");
        return true;
    }

    public void AdvanceToNextNightAndCarryState()
    {
        // Save current night
        SaveNight();

        // Increment night and clone the save forward (optional convenience)
        var json = SaveStorage.ReadJson(Slot);
        CurrentNight = Mathf.Clamp(CurrentNight + 1, 1, 5);
        SaveStorage.WriteJson(Slot, json);
    }

    public void SetCurrentNight(int night)
    {
        CurrentNight = night;
    }
}