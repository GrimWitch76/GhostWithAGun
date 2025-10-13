using UnityEngine;

public class InspectableObject : MonoBehaviour, IInteractable
{
    [Header("View Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _rotateSpeed = 5f;
    [SerializeField] private Vector3 _viewOffset = new Vector3(0f, 0f, 0.5f);
    [SerializeField] private float _viewDistance = 1.2f;

    private bool _isBeingViewed;
    private Transform _cam;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private float _lerpTime;
    private bool _returning;

    private void Awake()
    {
        _cam = Camera.main.transform;
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;
    }

    public void Interact(PlayerInteraction interactor)
    {
        if (_isBeingViewed)
        {
            // Return to world position
            _isBeingViewed = false;
            _returning = true;
            _lerpTime = 0f;
        }
        else
        {
            // Move into view
            _isBeingViewed = true;
            _returning = false;
            _lerpTime = 0f;
        }
    }

    public void GhostInteract(GameObject interactor) { }

    private void Update()
    {
        if (_isBeingViewed)
        {
            MoveIntoView();
        }
        else if (_returning)
        {
            MoveBackToOriginal();
        }
    }

    private void MoveIntoView()
    {
        _lerpTime += Time.deltaTime * _moveSpeed;

        Vector3 targetPos = _cam.position + _cam.forward * _viewDistance + _cam.TransformVector(_viewOffset);
        Quaternion targetRot = Quaternion.LookRotation(_cam.forward, Vector3.up);

        transform.position = Vector3.Lerp(transform.position, targetPos, _lerpTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * _rotateSpeed);
    }

    private void MoveBackToOriginal()
    {
        _lerpTime += Time.deltaTime * _moveSpeed;
        transform.position = Vector3.Lerp(transform.position, _originalPosition, _lerpTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, _originalRotation, Time.deltaTime * _rotateSpeed);

        if (Vector3.Distance(transform.position, _originalPosition) < 0.01f)
        {
            transform.position = _originalPosition;
            transform.rotation = _originalRotation;
            _returning = false;
        }
    }
}