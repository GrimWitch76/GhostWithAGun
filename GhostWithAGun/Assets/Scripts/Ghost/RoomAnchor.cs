using UnityEngine;

public class RoomAnchor : MonoBehaviour
{
    [Tooltip("Relative importance of this room for wandering (1 = normal, higher = more often chosen)")]
    public float weight = 1f;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}