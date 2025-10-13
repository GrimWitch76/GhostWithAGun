using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Moveable : MonoBehaviour, IInteractable, IDestructable
{
    [SerializeField] private bool _isHeavy = false;
    [SerializeField] private float _dragSpeed = 2f; // how fast you can pull a heavy item
    [SerializeField] private float _liftDistance = 2f; // distance from camera when held
    [SerializeField] private float _maxHealth;


    [SerializeField] public float value;
    private float _currentHealth;

    public bool IsHeavy => _isHeavy;
    public float DragSpeed => _dragSpeed;
    public float LiftDistance => _liftDistance;

    public void Interact(PlayerInteraction interactor)
    {
        interactor.TryPickup(this);
    }

    public void GhostInteract(GameObject interactor) { }

    public void TakeDamage(float amount)
    {
        throw new System.NotImplementedException();
    }

    public void Shatter()
    {
        throw new System.NotImplementedException();
    }
}