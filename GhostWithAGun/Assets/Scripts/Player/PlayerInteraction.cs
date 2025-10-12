using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float _interactDistance = 3f;
    [SerializeField] private LayerMask _interactMask;

    [SerializeField] private Transform _holdPoint; // empty GameObject in front of camera
    //[SerializeField] private float _verticalOffsetRange = 0.5f;

    [SerializeField] private float _minThrowForce = 5f;
    [SerializeField] private float _maxThrowForce = 20f;
    [SerializeField] private float _maxHoldTime = 1.5f;

    private float _holdStartTime;

    [SerializeField] private PlayerController _playerController;

    private Camera cam;
    private GameObject heldObject;
    private Rigidbody heldRb;
    private Moveable heldData;
    private bool isDraggingHeavy;
    private PlayerHealth _health;
    private SoundEmitter _heldEmitter;
    private void Awake()
    {
        cam = Camera.main;
    }

    public void TryInteractPressed(InputValue heldValue)
    {
        bool pressed = heldValue.isPressed;

        if (pressed)
        {
            _holdStartTime = Time.time;
        }
        else
        {
            float heldDuration = Time.time - _holdStartTime;

            if (heldObject == null)
            {
                if (_playerController.IsClimbing)
                {
                    _playerController.EndClimb();
                    return;
                }

                if (heldObject != null)
                {
                    DropObject();
                    return;
                }

                TryInteract(); // only try pickup if not holding anything
            }
            else
            {
                float holdRatio = Mathf.Clamp01(heldDuration / _maxHoldTime);
                float throwForce = Mathf.Lerp(_minThrowForce, _maxThrowForce, holdRatio);

                if (holdRatio < 0.1f)
                    DropObject();
                else
                    ThrowObject(throwForce);
            }
        }




    }

    private void TryInteract()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, _interactDistance, _interactMask))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(this);
            }
        }
        Debug.DrawRay(cam.transform.position, cam.transform.forward * _interactDistance, Color.red, 5f);
    }

    public void TryPickup(Moveable obj)
    {
        if (heldObject != null) return;

        heldObject = obj.gameObject;
        heldRb = obj.GetComponent<Rigidbody>();
        heldData = obj;
        heldRb.freezeRotation = true;

        if (heldData.IsHeavy)
        {
            // Dragging: stays on the ground
            _heldEmitter = obj.GetComponent<SoundEmitter>();
            isDraggingHeavy = true;
            heldRb.useGravity = true;
            heldRb.linearDamping = 8f;
            _playerController.StartDragHeavy(heldRb);
        }
        else
        {
            // Light: lifted off ground
            isDraggingHeavy = false;
            heldRb.useGravity = false;
            heldRb.linearDamping = 10f;
        }
    }

    private void FixedUpdate()
    {
        if (heldObject == null) return;

        if (isDraggingHeavy)
        {
            // Heavy object drag logic
            Vector3 targetPos = cam.transform.position + cam.transform.forward * 1.2f;
            targetPos.y = heldObject.transform.position.y; // keep ground level
            Vector3 moveDir = (targetPos - heldObject.transform.position);
            heldRb.AddForce(moveDir * heldData.DragSpeed, ForceMode.Acceleration);
        }
        else
        {
            // Light object hold logic
            Vector3 targetPos = _holdPoint.position;
            Vector3 moveDir = (targetPos - heldObject.transform.position);
            heldRb.linearVelocity = moveDir * 10f;
        }
    }

    public void DropObject()
    {
        if (heldRb == null) return;

        heldRb.useGravity = true;
        heldRb.linearDamping = 0f;
        heldRb.freezeRotation = false;

        heldObject = null;
        heldRb = null;
        heldData = null;
        isDraggingHeavy = false;
        _playerController.StopDragHeavy();
        _heldEmitter?.StopDragging();
        _heldEmitter = null;
    }

    public void ThrowObject(float throwForce)
    {
        if (heldRb == null || isDraggingHeavy) return; // can't throw heavy objects

        heldRb.useGravity = true;
        heldRb.linearDamping = 0f;
        heldRb.freezeRotation = false;
        heldRb.AddForce(cam.transform.forward * throwForce, ForceMode.Impulse);

        heldObject = null;
        heldRb = null;
        heldData = null;
    }

}
