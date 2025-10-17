using UnityEditor;
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
        Vector3 origin = transform.position + Vector3.up * 1f;
        Vector3 forward = transform.forward;
        float half = angle * 0.5f;

        bool sawPlayer = false;
        hitPos = default;

        for (int i = 0; i < _rayCount; i++)
        {
            float t = i / Mathf.Max(1f, (_rayCount - 1f));
            float a = Mathf.Lerp(-half, half, t);
            Vector3 dir = Quaternion.Euler(0, a, 0) * forward;

            // Perform raycast
            if (Physics.Raycast(origin, dir, out RaycastHit hit, range, _visionMask))
            {
                Debug.DrawLine(origin, hit.point, hit.transform.CompareTag("Player") ? Color.red : Color.green);

                if (hit.transform.CompareTag("Player"))
                {
                    hitPos = hit.transform.position;
                    sawPlayer = true;
                }
            }
            else
            {
                Debug.DrawRay(origin, dir * range, Color.gray);
            }
        }

        return sawPlayer;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_tuning == null) return;

        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 forward = transform.forward;

        // Primary cone
        Handles.color = new Color(1, 0, 0, 0.2f);
        Handles.DrawSolidArc(origin, Vector3.up,
            Quaternion.Euler(0, -_tuning.primaryConeAngle * 0.5f, 0) * forward,
            _tuning.primaryConeAngle, _tuning.primaryRange);

        // Secondary cone
        Handles.color = new Color(1, 1, 0, 0.15f);
        Handles.DrawSolidArc(origin, Vector3.up,
            Quaternion.Euler(0, -_tuning.secondaryConeAngle * 0.5f, 0) * forward,
            _tuning.secondaryConeAngle, _tuning.secondaryRange);

        // Forward line
        Handles.color = Color.white;
        Handles.DrawLine(origin, origin + forward * _tuning.primaryRange);
    }
#endif
}