using UnityEngine;

[RequireComponent(typeof(SaveableEntity))]
public class DoorSaveable : MonoBehaviour, ISaveable
{
    [SerializeField] private Door _door;

    // Hook these into your actual door code (e.g., hinge angle mapping).
    public object CaptureState()
    {
        return new DoorState
        {
            isOpen = _door.IsOpen,
            doorAngle = _door.GetCurrentAngle()
        };
    }

    public void RestoreState(object state)
    {
        var s = (DoorState)state;
        _door.SetHingePosition(s.doorAngle);
        _door.SetDoorOpen(s.isOpen);
    }
}