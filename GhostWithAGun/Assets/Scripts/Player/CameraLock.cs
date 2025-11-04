using Unity.Cinemachine;
using UnityEngine;

public class CameraLock : MonoBehaviour
{

    [SerializeField] CinemachineInputAxisController _controller;

    private void Start()
    {
        PlayerInteraction.LockCamera += LockCameraMovement;
    }

    private void OnDisable()
    {
        PlayerInteraction.LockCamera -= LockCameraMovement;
    }

    private void LockCameraMovement(bool locked)
    {
        _controller.Controllers[0].Enabled = !locked;
        _controller.Controllers[1].Enabled = !locked;
    }
}
