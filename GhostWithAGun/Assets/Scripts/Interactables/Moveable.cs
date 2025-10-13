using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Moveable : MonoBehaviour, IInteractable, IDestructable
{
    [SerializeField] private bool _isHeavy = false;
    [SerializeField] private float _dragSpeed = 2f; // how fast you can pull a heavy item
    [SerializeField] private float _liftDistance = 2f; // distance from camera when held

    [Header("Destruction")]
    [SerializeField] private bool _destructable;
    [SerializeField] private float _maxHealth;
    [SerializeField] private int _value;
    [SerializeField] private GameObject _destructablePrefab;

    private float _currentHealth;

    public bool IsHeavy => _isHeavy;
    public float DragSpeed => _dragSpeed;
    public float LiftDistance => _liftDistance;

    public float CurrentHealth => _currentHealth;

    public void Interact(PlayerInteraction interactor)
    {
        interactor.TryPickup(this);
    }

    public void SetHealth(float health)
    {
        _currentHealth = health;
    }

    public void GhostInteract(GameObject interactor) { }

    public void TakeDamage(float amount)
    {
        _currentHealth -= amount;
        if (_currentHealth <= 0)
        {
            Shatter();
        }
    }

    public void Shatter()
    {
        GameObject newObject = Instantiate(_destructablePrefab, transform.position, transform.rotation);
        gameObject.SetActive(false);
    }
}