using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Ladder : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform _topExitPoint;   // Optional: where to place player when exiting at top
    [SerializeField] private Transform _bottomExitPoint; // Optional: exit point for bottom
    [SerializeField] private float _attachOffset = 0.5f; // How far from the ladder surface to snap player

    public void Interact(PlayerInteraction interactor)
    {
        PlayerController player = interactor.GetComponent<PlayerController>();
        if (player == null) return;

        // Toggle climbing state
        if (!player.IsClimbing)
        {
            // Snap to ladder and begin climb
            Vector3 ladderForward = transform.forward;
            Vector3 attachPos = transform.position - ladderForward * _attachOffset;
            player.BeginClimb(this, attachPos, transform.up);
        }
        else
        {
            // Exit climbing
            player.EndClimb();
        }
    }

    public void GhostInteract(GameObject interactor)
    {
        // Not needed for now
    }

    public Transform TopExit => _topExitPoint;
    public Transform BottomExit => _bottomExitPoint;
}
