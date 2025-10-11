using UnityEngine;

#if UNITY_EDITOR
[ExecuteAlways]
#endif
public class SaveableEntity : MonoBehaviour
{
    [SerializeField] private string guid;   // stable, serialized

    public string Guid => guid;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(guid))
            guid = System.Guid.NewGuid().ToString("N");
    }
#endif
}

[System.Serializable]
public class MovableEntry
{
    public string guid;
    public MovableState state;
}

[System.Serializable]
public class DoorEntry
{
    public string guid;
    public DoorState state;
}

[System.Serializable]
public class LightEntry
{
    public string guid;
    public LightState state;
}