using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    [Header("Music Clips")]
    [SerializeField] private AudioClip _dayMusic;
    [SerializeField] private AudioClip _nightMusic;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _sourceA;
    [SerializeField] private AudioSource _sourceB;

    [Header("Transition Settings")]
    [SerializeField] private float _fadeTime = 2f;
    [SerializeField] private bool _crossFade = true;

    private bool _isDay = false;
    private AudioSource _activeSource;
    private AudioSource _inactiveSource;
    private Coroutine _transitionRoutine;

    private void Awake()
    {
        // Ensure both AudioSources exist
        if (!_sourceA) _sourceA = gameObject.AddComponent<AudioSource>();
        if (!_sourceB) _sourceB = gameObject.AddComponent<AudioSource>();

        _sourceA.loop = _sourceB.loop = true;

        _activeSource = _sourceA;
        _inactiveSource = _sourceB;
    }

    public void TransitionToDay()
    {
        if (_isDay) return;
        _isDay = true;
        StartTransition(_dayMusic);
    }

    public void TransitionToNight()
    {
        if (!_isDay) return;
        _isDay = false;
        StartTransition(_nightMusic);
    }

    private void StartTransition(AudioClip newClip)
    {
        if (_transitionRoutine != null)
            StopCoroutine(_transitionRoutine);

        _transitionRoutine = StartCoroutine(HandleTransition(newClip));
    }

    private IEnumerator HandleTransition(AudioClip newClip)
    {
        if (_crossFade)
        {
            // Prepare inactive source
            _inactiveSource.clip = newClip;
            _inactiveSource.volume = 0f;
            _inactiveSource.Play();

            float t = 0f;
            while (t < _fadeTime)
            {
                t += Time.deltaTime;
                float normalized = t / _fadeTime;

                _activeSource.volume = Mathf.Lerp(1f, 0f, normalized);
                _inactiveSource.volume = Mathf.Lerp(0f, 1f, normalized);

                yield return null;
            }

            _activeSource.Stop();

            // Swap roles
            (_activeSource, _inactiveSource) = (_inactiveSource, _activeSource);
        }
        else
        {
            // Fade out current track
            float t = 0f;
            while (t < _fadeTime)
            {
                t += Time.deltaTime;
                _activeSource.volume = Mathf.Lerp(1f, 0f, t / _fadeTime);
                yield return null;
            }

            _activeSource.Stop();
            PlayClip(newClip);

            // Fade back in
            t = 0f;
            while (t < _fadeTime)
            {
                t += Time.deltaTime;
                _activeSource.volume = Mathf.Lerp(0f, 1f, t / _fadeTime);
                yield return null;
            }
        }

        _transitionRoutine = null;
    }

    private void PlayClip(AudioClip clip)
    {
        _activeSource.clip = clip;
        _activeSource.volume = 1f;
        _activeSource.Play();
    }
}
