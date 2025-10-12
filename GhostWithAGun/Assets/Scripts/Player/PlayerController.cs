using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Parameters")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _crouchSpeed = 5f;
    [SerializeField] private float _sprintSpeed = 8f;
    [SerializeField] private float _staminaDrainSpeed = 2.5f;
    [SerializeField] private float _maxStamina = 5f;
    [SerializeField] private float _staminaRecoveryRate = 1f;
    [SerializeField] private float _climbSpeed = 3f;

    [Header("Jump Parameters")]
    [SerializeField] private float _jumpForce = 5f;

    [Header("Drag / Tether Settings")]
    [SerializeField] private float _dragMoveSpeedMultiplier = 0.5f; // base movement reduction while dragging
    [SerializeField] private float _tetherDistance = 2.0f;          // how far from the dragged object before slowdown
    [SerializeField] private float _tetherSoftZone = 0.5f;          // how far before hitting hard zero
    [SerializeField] private float _tetherLerpSpeed = 5f;
    [SerializeField] private float _tetherMinMultiplier = 0.2f; // Minimum movement speed when fully tensed


    [Header("Misc Parameters")]
    [SerializeField] private float _groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private float _customGravity = 20f;
    [SerializeField] private float _groundStickForce = 10f;



    public Vector3 MoveSpeed => _targetVelocity;
    public bool IsGrounded => _isGrounded;
    public bool IsClimbing => _isClimbing;


    private CharacterController _controller;
    private PlayerInputHandler _input;
    private PlayerHealth _health;
    private Vector3 _velocity;
    private Vector3 _targetVelocity;
    private RaycastHit _groundHit;
    private Rigidbody _rb;
    private Rigidbody _draggedRb;
    private Transform _cam;
    private float _currentStamina;
    private bool _isGrounded;
    private bool _jumpPressed;
    private bool _isClimbing;
    private bool _isDraggingHeavy;
    private Ladder _currentLadder;
    private Vector3 _ladderUp;
    private Vector3 _ladderForward;
    private Vector3 _ladderAttachPoint;
    private Vector3 _currentLadderPoint;
    private float _currentDragSpeedMultiplier = 1f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true; // prevent tipping over
        _health = GetComponent<PlayerHealth>();
        _input = GetComponent<PlayerInputHandler>();
        _cam = Camera.main.transform;
        _currentStamina = _maxStamina;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FixedUpdate()
    {
        if (_isClimbing)
        {
            // Move toward the ladder position smoothly to stay attached
            Vector3 desiredPos = _currentLadderPoint;
            transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * 10f);

            // Climb input
            float climbInput = _input.MoveInput.y;
            Vector3 climbVelocity = _ladderUp * climbInput * _climbSpeed;

            // Apply climb velocity directly
            _rb.linearVelocity = climbVelocity;

            // Align rotation with ladder
            Quaternion targetRot = Quaternion.LookRotation(-_ladderForward);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * 10f);
            _currentLadderPoint = transform.position;
            return; // Skip rest of movement while climbing
        }

        // --- Ground Check ---
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, out _groundHit, _groundCheckDistance + 0.2f, _groundMask);
        Vector3 groundNormal = _isGrounded ? _groundHit.normal : Vector3.up;

        // --- Camera-relative movement ---
        Vector3 camForward = Vector3.Scale(_cam.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = Vector3.Scale(_cam.right, new Vector3(1, 0, 1)).normalized;
        Vector3 moveInput = (camRight * _input.MoveInput.x + camForward * _input.MoveInput.y).normalized;

        // --- Speed selection ---
        float targetSpeed = _moveSpeed;
        if (_input.SprintHeld && _currentStamina > 0f)
        {
            targetSpeed = _sprintSpeed;
            _currentStamina = Mathf.Clamp(_currentStamina - (Time.deltaTime * _staminaDrainSpeed), 0, _maxStamina);
        }
        else if (_input.CrouchHeld)
        {
            targetSpeed = _crouchSpeed;
        }
        else
        {
            _currentStamina = Mathf.Clamp(_currentStamina + (Time.deltaTime * _staminaRecoveryRate), 0, _maxStamina);
        }

        // --- Project move input onto slope ---
        if (_isGrounded)
            moveInput = Vector3.ProjectOnPlane(moveInput, groundNormal).normalized;

        _targetVelocity = moveInput * targetSpeed;

        if (_isDraggingHeavy && _draggedRb != null)
        {
            // --- Base slowdown while dragging ---
            float baseMult = _dragMoveSpeedMultiplier;

            // --- Compute distance ratio for tether tension ---
            float dist = Vector3.Distance(transform.position, _draggedRb.position);
            float tension = Mathf.InverseLerp(_tetherDistance, _tetherDistance + _tetherSoftZone, dist);
            // tension = 0 when close, 1 when near limit

            // --- Lerp between base drag speed and minimum tether multiplier ---
            float targetMult = Mathf.Lerp(baseMult, _tetherMinMultiplier, tension);

            _currentDragSpeedMultiplier = Mathf.Lerp(_currentDragSpeedMultiplier, targetMult, Time.deltaTime * _tetherLerpSpeed);

            // --- Apply to movement velocity ---
            _targetVelocity *= _currentDragSpeedMultiplier;

        }
        else
        {
            // Return to normal speed smoothly
            _currentDragSpeedMultiplier = Mathf.Lerp(_currentDragSpeedMultiplier, 1f, Time.deltaTime * _tetherLerpSpeed);
        }

        // --- Preserve vertical velocity ---
        Vector3 velocity = _rb.linearVelocity;
        velocity.x = _targetVelocity.x;
        velocity.z = _targetVelocity.z;

        // --- Custom Gravity ---
        if (_isGrounded)
        {
            // Project gravity along surface normal to prevent sliding
            Vector3 slopeDown = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;
            Vector3 normalGravity = -groundNormal * _customGravity;
            _rb.AddForce(normalGravity, ForceMode.Acceleration);

            // Apply a little stick force to keep player glued to ground
            _rb.AddForce(-groundNormal * _groundStickForce, ForceMode.Acceleration);

            // Optional: damp any residual sliding
            Vector3 tangentVel = Vector3.ProjectOnPlane(_rb.linearVelocity, groundNormal);
            _rb.linearVelocity -= tangentVel * 0.05f; // tune 0.05–0.2 depending on feel
        }
        else
        {
            // Standard gravity when in air
            _rb.AddForce(Vector3.down * _customGravity, ForceMode.Acceleration);
        }

        _rb.linearVelocity = new Vector3(velocity.x, _rb.linearVelocity.y, velocity.z);
    }

    private void Update()
    {
        // --- Rotate player root with camera yaw ---
        Vector3 camForward = _cam.forward;
        camForward.y = 0f;
        if (camForward.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(camForward);
            transform.rotation = targetRot;
        }

        // --- Jump handling ---
        if (_jumpPressed && _isGrounded && !_input.CrouchHeld)
        {
            _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }

        if (_input.CrouchHeld && _isGrounded)
        {
            Debug.Log("Crouching");
        }

        _jumpPressed = false;
    }


    public void TryJump()
    {
        if (_isClimbing)
        {
            EndClimb();
            return;
        }
        _jumpPressed = true;
    }

    public void BeginClimb(Ladder ladder, Vector3 attachPoint, Vector3 ladderUp)
    {
        _isClimbing = true;
        _currentLadder = ladder;
        _ladderUp = ladderUp.normalized;
        _ladderForward = ladder.transform.forward;
        _ladderAttachPoint = attachPoint;
        _currentLadderPoint = attachPoint;
        _rb.useGravity = false;
        _rb.linearVelocity = Vector3.zero;

        // Optionally lock camera rotation or slow sensitivity here if needed
    }

    public void EndClimb()
    {
        if (!_isClimbing) return;
        _isClimbing = false;
        _rb.useGravity = true;
        _currentLadder = null;
    }

    public void StartDragHeavy(Rigidbody heavyRb)
    {
        _draggedRb = heavyRb;
        _isDraggingHeavy = true;
    }

    public void StopDragHeavy()
    {
        _draggedRb = null;
        _isDraggingHeavy = false;
    }

    private void OnDrawGizmosSelected()
    {
        // visualize ground check
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * (_groundCheckDistance + 0.1f));
    }

}
