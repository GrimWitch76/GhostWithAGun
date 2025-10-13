using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool CrouchHeld { get; private set; }
    public bool SprintHeld { get; private set; }

    [SerializeField] private PlayerController _playerController;
    [SerializeField] private PlayerInteraction _playerInteraction;
    [SerializeField] private FlashLight _flashLight;

    public void OnMove(InputValue value)
    {
        MoveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        LookInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        _playerController.TryJump();
    }

    public void OnInteract(InputValue value)
    {
        _playerInteraction.TryInteractPressed(value);
    }

    public void OnSprint(InputValue value)
    {
        SprintHeld = value.Get<float>() > 0.5f;
    }

    public void OnCrouch(InputValue value)
    {
        CrouchHeld = value.Get<float>() > 0.5f;
    }

    public void OnFlashlight(InputValue value)
    {
        _flashLight.ToggleLight();
    }
}
