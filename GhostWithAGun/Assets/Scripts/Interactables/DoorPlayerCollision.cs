using UnityEngine;

public class DoorPlayerCollision : MonoBehaviour
{
    [SerializeField] Rigidbody _rb;
    [SerializeField] SoundEmitter _doorSound;
    private void OnCollisionEnter(Collision collision)
    {
        // Allow physics push from player
        if (collision.collider.CompareTag("Player"))
        {
            if (_rb.angularVelocity.magnitude < 0.5f)
            {
                Vector3 pushDir = collision.relativeVelocity;
                float force = pushDir.magnitude * 0.5f;
                _doorSound.PlayOnce();
            }
        }
    }
}
