using UnityEngine;

[RequireComponent(typeof(SaveableEntity))]
public class LightSaveable : MonoBehaviour, ISaveable
{
    [SerializeField] private LightSwitch target;

    public object CaptureState()
    {
        LightState state = new LightState();
        state.IsOn = target.GetState();
        return state;
    }

    public void RestoreState(object state)
    {
        var s = (LightState)state;
        target.SetState(s.IsOn);
    }
}