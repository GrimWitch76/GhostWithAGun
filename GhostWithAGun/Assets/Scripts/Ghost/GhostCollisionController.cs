using UnityEngine;
public class GhostCollisionController : MonoBehaviour
{
    [SerializeField] private string ghostLayerName = "Ghost";
    [SerializeField] private string doorLayerName = "Door";

    private int ghostLayer;
    private int doorLayer;

    private void Awake()
    {
        ghostLayer = LayerMask.NameToLayer(ghostLayerName);
        doorLayer = LayerMask.NameToLayer(doorLayerName);
    }

    public void SetDoorCollision(bool enabled)
    {
        Physics.IgnoreLayerCollision(ghostLayer, doorLayer, enabled);
    }
}