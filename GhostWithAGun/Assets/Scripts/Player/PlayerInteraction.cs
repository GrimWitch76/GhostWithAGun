using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.UI.Image;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private LayerMask _interactMask;

    [SerializeField] private Transform _holdPoint; // empty GameObject in front of camera

    [SerializeField] private float _interactDistance = 3f;
    [SerializeField] private float _minThrowForce = 5f;
    [SerializeField] private float _maxThrowForce = 20f;
    [SerializeField] private float _maxHoldTime = 1.5f;
    [SerializeField] private float verticalOffsetRange = 0.5f;
    [SerializeField] private float _rotationSensitivity = 3.0f;
    [SerializeField] private float _pitchClamp = 85f;

    [SerializeField] private TextMeshProUGUI _interactTextUi;

    [SerializeField] private PlayerController _playerController;
    [SerializeField] private PlayerInputHandler _plyerInput;

    static public Action<bool> LockCamera;     

    private Camera _cam;
    private GameObject _heldObject;
    private Rigidbody _heldRb;
    private Moveable _heldData;
    private SoundEmitter _heldEmitter;
    private bool _isDraggingHeavy;
    private bool _cameraLocked;
    private float _holdStartTime;
    private float _accumYaw;
    private float _accumPitch;
    private string _interactText = "[E] Interact";
    private string _holdText = "[LMB] Pickup";
    private void Awake()
    {
        _cam = Camera.main;
    }

    //Handle interactions
    public void TryInteractPressed(InputValue heldValue)
    {
        TryInteract(); //Yeah just interact

    }

    //Handle holding
    public void TryHoldPressed(bool held)
    {
        if(held)
        {
            StartHold();
        }
        else
        {
            DropObject();
        }
    }

    //Handle updating the hud
    private void Update()
    {
        if(_heldObject != null && _plyerInput.RotateHeld)
        {
            if(!_cameraLocked)
            {
                _cameraLocked = true;
                LockCamera?.Invoke(_cameraLocked);
            }
            ApplyHeldRotation(_plyerInput.LookInput);
        }
        else
        {
            _cameraLocked = false;
            LockCamera?.Invoke(_cameraLocked);
        }

        _interactTextUi.text = "";

        Vector3 origin = _cam.transform.position;
        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, _interactDistance, _interactMask))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                _interactTextUi.text = _interactText;
            }

            Moveable moveable = hit.collider.GetComponent<Moveable>();
            if(moveable != null)
            {
                _interactTextUi.text = _holdText;
            }
            return;
        }

        // Fallback check if inside an interactable volume
        Collider[] hits = Physics.OverlapSphere(origin, 0.3f, _interactMask);
        foreach (var h in hits)
        {
            if (h.TryGetComponent<IInteractable>(out var interactable))
            {
                _interactTextUi.text = _interactText;

                break;
            }

            Moveable moveable = hit.collider.GetComponent<Moveable>();
            if (moveable != null)
            {
                _interactTextUi.text = _holdText;
                break;
            }
        }
    }

    private void TryInteract()
    {
        Vector3 origin = _cam.transform.position;
        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, _interactDistance, _interactMask))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(this);
            }
        }
        Debug.DrawRay(_cam.transform.position, _cam.transform.forward * _interactDistance, Color.red, 5f);

        // Fallback check if inside an interactable volume
        Collider[] hits = Physics.OverlapSphere(origin, 0.3f, _interactMask);
        foreach (var h in hits)
        {
            if (h.TryGetComponent<IInteractable>(out var interactable))
            {
                interactable.Interact(this);
                break;
            }
        }
    }

    private void StartHold()
    {
        Vector3 origin = _cam.transform.position;
        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, _interactDistance, _interactMask))
        {
            Moveable moveable = hit.collider.GetComponent<Moveable>();
            if (moveable != null)
            {
                moveable.Pickup(this);
            }
        }
        Debug.DrawRay(_cam.transform.position, _cam.transform.forward * _interactDistance, Color.yellow, 5f);

        // Fallback check if inside an interactable volume
        Collider[] hits = Physics.OverlapSphere(origin, 0.3f, _interactMask);
        foreach (var h in hits)
        {
            if (h.TryGetComponent<Moveable>(out var interactable))
            {
                interactable.Pickup(this);
                break;
            }
        }
    }

    public void TryPickup(Moveable obj)
    {
        if (_heldObject != null) return;

        _heldObject = obj.gameObject;
        _heldRb = obj.GetComponent<Rigidbody>();
        _heldData = obj;
        _heldRb.freezeRotation = true;

        if (_heldData.IsHeavy)
        {
            // Dragging: stays on the ground
            _heldEmitter = obj.GetComponent<SoundEmitter>();
            _isDraggingHeavy = true;
            _heldRb.useGravity = true;
            _heldRb.linearDamping = 8f;
            _playerController.StartDragHeavy(_heldRb);
        }
        else
        {
            // Light: lifted off ground
            _isDraggingHeavy = false;
            _heldRb.useGravity = false;
            _heldRb.linearDamping = 10f;
        }
    }

    private void FixedUpdate()
    {
        if (_heldObject == null) return;

        if (_isDraggingHeavy)
        {
            // Heavy object drag logic
            Vector3 targetPos = _cam.transform.position + _cam.transform.forward * 1.2f;
            targetPos.y = _heldObject.transform.position.y; // keep ground level
            Vector3 moveDir = (targetPos - _heldObject.transform.position);
            _heldRb.AddForce(moveDir * _heldData.DragSpeed, ForceMode.Acceleration);
        }
        else
        {
            // Light object hold logic
            Vector3 targetPos = _holdPoint.position;
            Vector3 moveDir = (targetPos - _heldObject.transform.position);
            _heldRb.linearVelocity = moveDir * 10f;
        }
    }


    private void LateUpdate()
    {
        // Base hold position directly in front of the camera
        _holdPoint.position = _cam.transform.position + _cam.transform.forward * 2f;

        // Get pitch from camera (local X angle)
        float pitch = _cam.transform.localEulerAngles.x;
        if (pitch > 180f) pitch -= 360f; // wrap so -90..90

        // Normalize pitch to -1..1
        float normalizedPitch = Mathf.Clamp(pitch / 90f, -1f, 1f);

        // Apply vertical offset
        Vector3 offset = Vector3.up * (normalizedPitch * verticalOffsetRange);
        _holdPoint.position += offset;

        // Always face forward
        _holdPoint.rotation = _cam.transform.rotation;
    }


    public void DropObject()
    {
        if (_heldRb == null) return;

        _heldRb.useGravity = true;
        _heldRb.linearDamping = 0f;
        _heldRb.freezeRotation = false;

        _heldObject = null;
        _heldRb = null;
        _heldData = null;
        _isDraggingHeavy = false;
        _playerController.StopDragHeavy();
        _heldEmitter?.StopDragging();
        _heldEmitter = null;
    }

    private void ApplyHeldRotation(Vector2 lookDelta)
    {
        if (_heldObject == null || _isDraggingHeavy) return;

        Vector3 yawAxis = _cam.transform.up;     // left/right
        Vector3 pitchAxis = _cam.transform.right;  // up/down

        float yawDegrees = -lookDelta.x * _rotationSensitivity;
        float pitchDegrees = lookDelta.y * _rotationSensitivity;

        Quaternion qYaw = Quaternion.AngleAxis(yawDegrees, yawAxis);
        Quaternion qPitch = Quaternion.AngleAxis(pitchDegrees, pitchAxis);

        // Apply incrementally so it’s purely relative to camera each frame
        _heldObject.transform.rotation = qYaw * qPitch * _heldObject.transform.rotation;
    }

}
