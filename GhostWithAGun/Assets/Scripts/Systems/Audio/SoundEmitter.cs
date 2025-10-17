using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundEmitter : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip[] _audioClips;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField, Range(0f, 1f)] private float _minVolume = 0.2f;
    [SerializeField, Range(0f, 1f)] private float _maxVolume = 1f;

    [Header("Noise Broadcast Settings")]
    [SerializeField] private float _baseLoudness = 1f;
    [SerializeField] private float _maxLoudness = 10f;
    [SerializeField] private float _importance = 1f;
    [SerializeField] private float _surfaceModifier = 1f; // Adjust per-material

    [Header("Collision Settings")]
    [SerializeField] private bool _broadcastOnCollision;
    [SerializeField] private float _velocityScale = 1f;
    [SerializeField] private float _minVelocityThreshold = 1f;

    private float _loopBroadcastInterval = 1f;
    private float _broadcastTimer;
    private bool _isDragging;
    private bool _looping;
    private Vector3 _lastPos;

    private void Awake()
    {
        if (!_audioSource)
            _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    private void Update()
    {
        // For looping sounds that should keep broadcasting while active
        if (_looping && _audioSource.isPlaying)
        {
            _broadcastTimer += Time.deltaTime;
            if (_broadcastTimer >= _loopBroadcastInterval)
            {
                BroadCastNoise(_baseLoudness);
                _broadcastTimer = 0f;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!_broadcastOnCollision)
            return;

        float impactSpeed = collision.relativeVelocity.magnitude;
        if (impactSpeed < _minVelocityThreshold)
            return;

        // Combine surface modifiers from both colliders
        float otherSurface = 1f;
        if (collision.collider.TryGetComponent(out SoundEmitter otherEmitter))
            otherSurface = otherEmitter._surfaceModifier;

        float loudness = Mathf.Clamp(
            (_baseLoudness + impactSpeed * _velocityScale) * _surfaceModifier * otherSurface,
            _baseLoudness,
            _maxLoudness
        );

        PlaySound(loudness);
        BroadCastNoise(loudness);
    }

    /// <summary>Plays and broadcasts a single sound event with default loudness.</summary>
    public void PlayOnce(float loudness = 1, bool broadCast = true)
    {
        PlaySound(loudness);
        if(broadCast)
            BroadCastNoise(_baseLoudness);
    }

    public void PlayOnce()
    {
        PlaySound(_baseLoudness, false);
    }

    /// <summary>Starts a looping sound and periodically broadcasts noise while playing.</summary>
    public void PlayLooped()
    {
        _audioSource.loop = true;
        _looping = true;
        PlaySound(_baseLoudness);
        BroadCastNoise(_baseLoudness);
    }

    /// <summary>Stops any currently playing sound and looping broadcast.</summary>
    public void Stop()
    {
        _audioSource.Stop();
        _looping = false;
    }

    public void StartDragging()
    {
        _isDragging = true;
        _lastPos = transform.position;
        PlayLooped(); // reuse existing logic
    }

    public void StopDragging()
    {
        _isDragging = false;
        Stop();
    }

    private void PlaySound(float loudness, bool adjustDistance = true)
    {
        if (_audioClips.Length == 0)
            return;

        _audioSource.clip = GetRandomClip();
        _audioSource.volume = Normalize(loudness, _minVolume, _maxVolume, _minVolume, _maxVolume);
        if(adjustDistance) _audioSource.maxDistance = Normalize(loudness, _baseLoudness, _maxLoudness, 5f, 25f);
        _audioSource.Play();
    }

    private void BroadCastNoise(float loudness)
    {
        var sound = new SoundEvent(transform.position, loudness, _importance, gameObject);
        SoundManager.EmitSound(sound);

#if UNITY_EDITOR
        SoundGizmoDrawer.DrawSound(sound.Position, sound.Loudness, 3f);
#endif
    }

    private float Normalize(float value, float inMin, float inMax, float outMin, float outMax)
    {
        float t = Mathf.InverseLerp(inMin, inMax, value);
        return Mathf.Lerp(outMin, outMax, t);
    }

    private AudioClip GetRandomClip()
    {
        return _audioClips[Random.Range(0, _audioClips.Length - 1)];
    }

    private void LateUpdate()
    {
        if (!_isDragging) return;

        float speed = (transform.position - _lastPos).magnitude / Time.deltaTime;
        _lastPos = transform.position;

        // Map speed to loudness (adjust these to taste)
        float dynamicLoudness = Mathf.Clamp(_baseLoudness + speed * 0.1f, _baseLoudness, _maxLoudness);
        _audioSource.volume = Normalize(dynamicLoudness, _baseLoudness, _maxLoudness, _minVolume, _maxVolume);

        // Optionally broadcast every few frames or seconds
        _broadcastTimer += Time.deltaTime;
        if (_broadcastTimer >= _loopBroadcastInterval)
        {
            BroadCastNoise(dynamicLoudness);
            _broadcastTimer = 0f;
        }
    }
}
