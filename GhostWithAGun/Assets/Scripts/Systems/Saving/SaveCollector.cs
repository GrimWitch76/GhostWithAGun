using UnityEngine;

using System.Collections.Generic;

public class SaveCollector : MonoBehaviour
{
    public SaveEnvelope CaptureAll(int night)
    {
        var env = new SaveEnvelope { night = night };

        foreach (var se in FindObjectsByType<SaveableEntity>(FindObjectsSortMode.None))
        {
            foreach (var s in se.GetComponents<ISaveable>())
            {
                var guid = se.Guid;

                switch (s)
                {
                    case MoveableSaveable m:
                        env.movables.Add(new MovableEntry
                        {
                            guid = se.Guid,
                            state = (MovableState)m.CaptureState()
                        });
                        break;
                    case DoorSaveable d:
                        env.doors.Add(new DoorEntry
                        {
                            guid = se.Guid,
                            state = (DoorState)d.CaptureState()
                        });
                        break;
                    case LightSaveable l:
                        env.lights.Add(new LightEntry
                        {
                            guid = se.Guid,
                            state = (LightState)l.CaptureState()
                        });
                        break;
                }
            }
        }
        return env;
    }

    public void ApplyAll(SaveEnvelope env)
    {
        // Map for quick lookups
        var map = new Dictionary<string, SaveableEntity>();
        foreach (var se in FindObjectsByType<SaveableEntity>(FindObjectsSortMode.None))
            map[se.Guid] = se;

        foreach (var e in env.movables)
        {
            if (map.TryGetValue(e.guid, out var se))
                foreach (var s in se.GetComponents<ISaveable>())
                    if (s is MoveableSaveable) s.RestoreState(e.state);
        }

        foreach (var e in env.doors)
        {
            if (map.TryGetValue(e.guid, out var se))
                foreach (var s in se.GetComponents<ISaveable>())
                    if (s is DoorSaveable) s.RestoreState(e.state);
        }
        foreach (var e in env.lights)
        {
            if (map.TryGetValue(e.guid, out var se))
                foreach (var s in se.GetComponents<ISaveable>())
                    if (s is LightSaveable) s.RestoreState(e.state);
        }
    }
}