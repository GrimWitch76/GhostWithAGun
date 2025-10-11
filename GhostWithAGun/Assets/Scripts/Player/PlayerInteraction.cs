using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float _interactDistance = 3f;
    [SerializeField] private LayerMask _interactMask;

    [SerializeField] private Transform _holdPoint; // empty GameObject in front of camera
    [SerializeField] private float _verticalOffsetRange = 0.5f;
    [SerializeField] private float _throwForce = 5f;

    [SerializeField] private PlayerController _playerController;

    private Camera cam;
    private GameObject heldObject;
    private Rigidbody heldRb;
    private PlayerHealth _health;

    private void Awake()
    {
        cam = Camera.main;
    }

    public void TryInteractPressed(float heldValue)
    {
        if (_playerController.IsClimbing)
        {
            _playerController.EndClimb();
            return;
        }

        TryInteract(heldValue); // only try pickup if not holding anything
    }

    private void TryInteract(float heldValue)
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, _interactDistance, _interactMask))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(this, heldValue);
            }
        }
        Debug.DrawRay(cam.transform.position, cam.transform.forward * _interactDistance, Color.red, 5f);
    }

}
