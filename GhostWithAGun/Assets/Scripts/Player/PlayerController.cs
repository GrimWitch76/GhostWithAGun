using System.Reflection;
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

    [Header("Jump Parameters")]
    [SerializeField] private float _airControlMultipler = 0.25f;
    [SerializeField] private float _jumpForce = 5f;

    [Header("Misc Parameters")]
    [SerializeField] private float _groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask _groundMask;

    public Vector3 MoveSpeed => _targetVelocity;
    public bool IsGrounded => _isGrounded;


    private CharacterController _controller;
    private PlayerInputHandler _input;
    private PlayerHealth _health;
    private Vector3 _velocity;
    private Vector3 _targetVelocity;
    private Rigidbody _rb;
    private Transform _cam;
    private float _currentStamina;
    private bool _isGrounded;
    private bool _jumpPressed;

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
        transform.localScale = new Vector3(1, 1f, 1);

        // --- Ground Check ---
        _isGrounded = Physics.Raycast(transform.position, Vector3.down,
            _groundCheckDistance + 0.1f, _groundMask);

        // --- Camera-relative movement ---
        Vector3 camForward = Vector3.Scale(_cam.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = Vector3.Scale(_cam.right, new Vector3(1, 0, 1)).normalized;

        Vector3 move = (camRight * _input.MoveInput.x + camForward * _input.MoveInput.y).normalized;

        Debug.Log(_input.MoveInput.y);
        if (_input.SprintHeld && _currentStamina > 0f)
        {
            _targetVelocity = move * _sprintSpeed;
            _currentStamina = Mathf.Clamp(_currentStamina - (Time.deltaTime * _staminaRecoveryRate), 0, _maxStamina);
        }
        else if (_input.CrouchHeld)
        {
            transform.localScale = new Vector3(1, 0.5f, 1);
            _targetVelocity = move * _crouchSpeed;
        }
        else
        {
            _targetVelocity = move * _moveSpeed;
        }

        if (!_isGrounded)
        {
            //_targetVelocity = _targetVelocity * _airControlMultipler;
        }

        if (!_input.SprintHeld)
        {
            _currentStamina = Mathf.Clamp(_currentStamina + (Time.deltaTime * _staminaRecoveryRate), 0, _maxStamina);
        }

        // keep current vertical velocity
        Vector3 vel = _rb.linearVelocity;
        vel.x = _targetVelocity.x;
        vel.z = _targetVelocity.z;
        _rb.linearVelocity = vel;
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
        _jumpPressed = true;
    }

    private void OnDrawGizmosSelected()
    {
        // visualize ground check
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * (_groundCheckDistance + 0.1f));
    }

}
