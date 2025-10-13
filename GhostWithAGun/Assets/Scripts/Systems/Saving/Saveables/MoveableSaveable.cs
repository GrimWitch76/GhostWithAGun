using UnityEngine;

[RequireComponent(typeof(SaveableEntity))]
public class MoveableSaveable : MonoBehaviour, ISaveable
{
    [SerializeField] private Rigidbody rb;

    public object CaptureState()
    {
        return new MovableState
        {
            pos = new[] { transform.position.x, transform.position.y, transform.position.z },
            rot = new[] { transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w },
            health = rb.GetComponent<Moveable>().CurrentHealth,
            destroyed = !gameObject.activeSelf
        };
    }

    public void RestoreState(object state)
    {
        var s = (MovableState)state;
        var moveable = rb.GetComponent<Moveable>();
        if (s.destroyed) { moveable.Shatter();  return; }

        transform.SetPositionAndRotation(
            new Vector3(s.pos[0], s.pos[1], s.pos[2]),
            new Quaternion(s.rot[0], s.rot[1], s.rot[2], s.rot[3])
        );
        moveable.SetHealth(s.health);

        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.Sleep(); 
        }
    }
}
