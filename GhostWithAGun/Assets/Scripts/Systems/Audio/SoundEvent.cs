using UnityEngine;

public struct SoundEvent
{
    public readonly Vector3 Position;
    public readonly float Loudness;
    public readonly float Importance;
    public readonly GameObject Source;

    public SoundEvent(Vector3 position, float loudness, float importance, GameObject source)
    {
        Position = position;
        Loudness = loudness;
        Importance = importance;
        Source = source;
    }
}