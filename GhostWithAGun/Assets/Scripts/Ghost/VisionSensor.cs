using UnityEngine;

public class VisionSensor : MonoBehaviour
{
    public bool CanSeePlayer { get; private set; }
    public Vector3 LastSeenPosition { get; private set; }

    [SerializeField] private LayerMask _visionMask;
    [SerializeField] private int _rayCount = 30;

    private GhostBrain _brain;
    private GhostTuning _tuning;
    private PlayerController _player;

    public void ApplyTuning(GhostTuning tuning)
    {
        _tuning = tuning;
    }

    void Start()
    {
        _brain = GetComponent<GhostBrain>();
    }

    void Update()
    {
        CanSeePlayer = false;

        if (DrawVisionCone(_tuning.primaryConeAngle, _tuning.primaryRange, out var pos))
        {
            CanSeePlayer = true;
            LastSeenPosition = pos;
            _brain.AddSuspicion(ScaledVisionSuspicion(_tuning.suspicionPerPrimaryVision), LastSeenPosition);
        }
        else if (DrawVisionCone(_tuning.secondaryConeAngle, _tuning.secondaryRange, out pos))
        {
            CanSeePlayer = true;
            LastSeenPosition = pos;
            _brain.AddSuspicion(ScaledVisionSuspicion(_tuning.suspicionPerSecondaryVision), LastSeenPosition);
        }
    }

    private float ScaledVisionSuspicion(float baseAdd)
    {
        float m = 1f;

        if (_player != null && _player.IsCrouching)
            m *= _tuning.crouchVisibilityMultiplier;

        return baseAdd * m;
    }

    private bool DrawVisionCone(float angle, float range, out Vector3 hitPos)
    {
        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 forward = transform.forward;
        float half = angle * 0.5f;

        for (int i = 0; i < _rayCount; i++)
        {
            float t = i / Mathf.Max(1f, (_rayCount - 1f));
            float a = Mathf.Lerp(-half, half, t);
            Vector3 dir = Quaternion.Euler(0, a, 0) * forward;

            if (Physics.Raycast(origin, dir, out RaycastHit hit, range, _visionMask))
            {
                if (hit.transform.CompareTag("Player"))
                {
                    hitPos = hit.transform.position;
                    return true;
                }
            }
        }

        hitPos = default;
        return false;
    }
}