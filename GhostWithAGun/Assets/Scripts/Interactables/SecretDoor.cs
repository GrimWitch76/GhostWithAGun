using UnityEngine;

public class SecretDoor : MonoBehaviour, IInteractable
{
    [Header("Refs")]
    [SerializeField] private SoundEmitter _soundEmitter;
    [SerializeField] private Transform _hatch;

    [Header("Motion")]
    [SerializeField, Tooltip("Closed local X angle in degrees")]
    private float closedX = 0f;

    [SerializeField, Tooltip("Open local X angle in degrees (e.g., -90 for downward hatch)")]
    private float openX = -90f;

    [SerializeField, Tooltip("Seconds to fully open/close")]
    private float duration = 0.45f;

    [SerializeField]
    private AnimationCurve curve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private bool _isOpen;
    private Coroutine _anim;

    public void GhostInteract(GameObject ghost)
    {
        ToggleOpen();
    }

    public void Interact(PlayerInteraction interactor)
    {
        ToggleOpen();
    }

    private void ToggleOpen()
    {
        if (_hatch == null) return;

        // Optional: play sfx
        _soundEmitter?.PlayOnce();

        // Flip target state
        _isOpen = !_isOpen;

        // Restart animation if already moving
        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(AnimateHatch(_isOpen));
    }

    private System.Collections.IEnumerator AnimateHatch(bool open)
    {
        // Capture start & end rotations using local space
        Vector3 startEuler = _hatch.localEulerAngles;
        // Normalize X so 350° becomes -10°, avoiding the long-way-around
        startEuler.x = NormalizeDegrees(startEuler.x);

        float targetX = open ? openX : closedX;
        Vector3 endEuler = new Vector3(targetX, startEuler.y, startEuler.z);

        Quaternion from = Quaternion.Euler(startEuler);
        Quaternion to = Quaternion.Euler(endEuler);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, duration);
            float k = curve.Evaluate(Mathf.Clamp01(t));
            _hatch.localRotation = Quaternion.Lerp(from, to, k);
            yield return null;
        }

        // Snap to final to avoid float drift
        _hatch.localRotation = to;
        _anim = null;
    }

    private static float NormalizeDegrees(float deg)
    {
        // Map [0..360) to (-180..180]
        deg %= 360f;
        if (deg > 180f) deg -= 360f;
        if (deg <= -180f) deg += 360f;
        return deg;
    }
}
