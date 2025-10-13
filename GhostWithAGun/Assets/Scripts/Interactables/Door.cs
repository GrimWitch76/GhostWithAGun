using UnityEngine;
using UnityEngine.AI;

public class Door : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    [SerializeField] private float _openForce = 5f;
    [SerializeField] private float _closeForce = 15f;     // PD proportional gain
    [SerializeField] private float _closeDamping = 2f;    // PD derivative damping
    [SerializeField] private float _stopAngleEpsilon = 1f;
    [SerializeField] private float _stopVelocityEpsilon = 0.1f;

    [Header("Barricade Settings")]
    [SerializeField] private float _barricadeCheckRadius = 0.6f;
    [SerializeField] private float _barricadeCheckDepth = 0.3f;
    [SerializeField] private float _minBarricadeMass = 8f;
    [SerializeField] private float _minBarricadeHeight = 0.3f;
    [SerializeField] private float _unbarricadeDelay = 0.5f;
    [SerializeField] private float _barricadeCheckDelay = 0.5f; //check every half a second.
    [SerializeField] private NavMeshObstacle _navObstacle;

    [SerializeField] private SoundEmitter _doorSound;
    [SerializeField] private Transform _door;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private HingeJoint _hinge;
    private bool _isOpen;
    private bool _isClosing;
    private bool _isBarricaded;
    private float _targetAngle = 0f;
    private float _barricadeTimer;
    public bool IsOpen => _isOpen;
    public bool IsBarricaded => _isBarricaded;
    private void Awake()
    {
        _rb.maxAngularVelocity = 10f;
        _navObstacle.carving = false;
        _navObstacle.enabled = false;
    }

    public void Interact(PlayerInteraction interactor)
    {
        Vector3 toDoor = (_door.position - interactor.transform.position).normalized;
        Vector3 doorForward = _door.forward;
        float dot = Vector3.Dot(toDoor, doorForward);
        float direction = dot > 0 ? -1f : 1f;
        _doorSound.PlayOnce();

        if (_isOpen)
            BeginClose();
        else
            OpenDoor(direction);
    }

    public void GhostInteract(GameObject ghost)
    {
        Vector3 toDoor = (_door.position - ghost.transform.position).normalized;
        Vector3 doorForward = _door.forward;
        float dot = Vector3.Dot(toDoor, doorForward);
        float direction = dot > 0 ? -1f : 1f;
        _doorSound.PlayOnce(1, false);

        OpenDoor(direction);
    }

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

        UpdateBarricadeStatus();
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
                _doorSound.PlayOnce();
            }
        }
    }

    public float GetCurrentAngle()
    {
        return _door.localEulerAngles.y;
    }

    private void UpdateBarricadeStatus()
    {
        bool blocked = CheckForBarricade();

        if (blocked)
        {
            _isBarricaded = true;
            _barricadeTimer = 0f;
            _navObstacle.enabled = true;
            _navObstacle.carving = true;
        }
        else if (_isBarricaded)
        {
            _barricadeTimer += Time.fixedDeltaTime;
            if (_barricadeTimer >= _unbarricadeDelay)
                _isBarricaded = false;
            _navObstacle.carving = false;
            _navObstacle.enabled = false;
        }
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

    private bool CheckForBarricade()
    {
        // Door forward points *outward* from the hinge pivot side
        Vector3 doorCenter = _door.position;
        Vector3 halfExtents = new Vector3(_barricadeCheckRadius, 1f, _barricadeCheckDepth);

        // Create a small overlap box just in front of the door plane
        Collider[] hits = Physics.OverlapBox(
            doorCenter,
            halfExtents,
            _door.rotation,
            ~0, // all layers (optional: restrict to props)
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider hit in hits)
        {
            if (hit.attachedRigidbody == null) continue;
            Rigidbody rb = hit.attachedRigidbody;
            if (rb == _rb) continue; // ignore the door itself

            // Check mass
            if (rb.mass < _minBarricadeMass) continue;

            // Check vertical height (top of object)
            Bounds bounds = hit.bounds;
            float height = bounds.max.y - transform.position.y;
            if (height < _minBarricadeHeight) continue;

            // Optional: ensure it’s actually close to the door plane
            Vector3 closest = hit.ClosestPoint(doorCenter);
            float dist = Vector3.Dot(closest - doorCenter, _door.forward);
            if (Mathf.Abs(dist) > _barricadeCheckDepth) continue;

            // All checks passed — consider this a barricade
            return true;
        }

        return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_door == null) return;
        Gizmos.color = _isBarricaded ? Color.red : Color.green;
        Vector3 center = _door.position;
        Vector3 size = new Vector3(_barricadeCheckRadius * 2, 2f, _barricadeCheckDepth * 2);
        Gizmos.matrix = Matrix4x4.TRS(center, _door.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, size);
    }
#endif
}
