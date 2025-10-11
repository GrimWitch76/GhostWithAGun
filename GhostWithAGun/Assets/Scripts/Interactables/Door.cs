using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    [SerializeField] private float _openForce = 5f;
    [SerializeField] private float _closeForce = 15f;     // PD proportional gain
    [SerializeField] private float _closeDamping = 2f;    // PD derivative damping
    [SerializeField] private float _stopAngleEpsilon = 1f;
    [SerializeField] private float _stopVelocityEpsilon = 0.1f;

    [SerializeField] private Transform _door;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private HingeJoint _hinge;
    private bool _isOpen;
    private bool _isClosing;
    private float _targetAngle = 0f;

    public bool IsOpen => _isOpen;
    private void Awake()
    {
        _rb.maxAngularVelocity = 10f;
    }

    public void Interact(PlayerInteraction interactor)
    {
        Vector3 toDoor = (_door.position - interactor.transform.position).normalized;
        Vector3 doorForward = _door.forward;
        float dot = Vector3.Dot(toDoor, doorForward);
        float direction = dot > 0 ? -1f : 1f;

        if (_isOpen)
            BeginClose();
        else
            OpenDoor(direction);
    }

    public void GhostInteract(GhostInteraction interactor) { }

    private void OpenDoor(float direction)
    {
        _isClosing = false;
        _rb.angularVelocity = Vector3.zero; 
        _rb.AddTorque(_door.up * direction * _openForce, ForceMode.VelocityChange);
        _isOpen = true;
    }

    private void BeginClose()
    {
        _isClosing = true;
        _isOpen = false;
    }

    private void FixedUpdate()
    {
        if (_isClosing)
            ApplyClosePD();
    }

    private void ApplyClosePD()
    {
        float currentAngle = _hinge.angle;
        float angleError = _targetAngle - currentAngle;
        float absError = Mathf.Abs(angleError);

        float torque = angleError * _closeForce - _rb.angularVelocity.y * _closeDamping;

        if (absError < 10f)
        {
            float latchStrength = Mathf.InverseLerp(10f, 0f, absError); 
            float latchTorque = Mathf.Sign(angleError) * latchStrength * _closeForce * 3f;
            torque += latchTorque;

            _rb.angularVelocity *= Mathf.Lerp(1f, 0.9f, latchStrength);
        }

        _rb.AddTorque(_door.up * torque, ForceMode.Acceleration);

        bool nearlyClosed = absError < _stopAngleEpsilon;
        bool nearlyStopped = Mathf.Abs(_rb.angularVelocity.y) < _stopVelocityEpsilon;

        if (nearlyClosed && nearlyStopped)
        {
            _rb.angularVelocity = Vector3.zero;
            _rb.Sleep();

            Vector3 euler = _door.localEulerAngles;
            euler.y = _targetAngle;
            _door.localEulerAngles = euler;

            _isClosing = false;
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        // Allow physics push from player
        if (collision.collider.CompareTag("Player"))
        {
            if (_rb.angularVelocity.magnitude < 0.5f)
            {
                Vector3 pushDir = collision.relativeVelocity;
                float force = pushDir.magnitude * 0.5f;
                _rb.AddTorque(_door.up * force, ForceMode.Impulse);
            }
        }
    }

    public float GetCurrentAngle()
    {
        return _door.localEulerAngles.y;
    }

    public void SetHingePosition(float position)
    {
        // Temporarily disable physics
        _rb.isKinematic = true;

        // Get the hinge’s anchor and axis in world space
        Vector3 hingeAnchorWorld = transform.TransformPoint(_hinge.anchor);
        Vector3 hingeAxisWorld = transform.TransformDirection(_hinge.axis);

        // Compute desired rotation around hinge
        Quaternion rotation = Quaternion.AngleAxis(position, hingeAxisWorld);

        // Rotate the door around the hinge anchor
        Vector3 doorPos = _door.position;
        Vector3 dirFromAnchor = doorPos - hingeAnchorWorld;
        dirFromAnchor = rotation * dirFromAnchor;
        Vector3 newPos = hingeAnchorWorld + dirFromAnchor;

        // Apply rotation and position to the door
        _door.SetPositionAndRotation(newPos, rotation * _door.rotation);

        // Zero velocities and re-enable physics
        _rb.angularVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.isKinematic = false;
        _rb.Sleep();
    }

    public void SetDoorOpen(bool open)
    {
        _isOpen = open;
    }
}
