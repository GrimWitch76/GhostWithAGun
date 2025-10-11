using UnityEngine;

public interface IInteractable
{
    void Interact(PlayerInteraction interactor);
    void GhostInteract(GhostInteraction interactor);
}
