using UnityEngine;

public interface IInteractable
{
    void Interact(PlayerInteraction interactor, float heldValue);
    void GhostInteract(GhostInteraction interactor);
}
