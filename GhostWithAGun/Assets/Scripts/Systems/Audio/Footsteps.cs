using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

public class Footsteps : MonoBehaviour
{
    [SerializeField] private SoundEmitter _soundEmitter;
    [SerializeField] private float baseStepInterval = 0.5f; // interval at baseSpeed
    [SerializeField] private float baseSpeed = 5f;          // your "normal walking" speed
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private NavMeshAgent _agent;
    private PlayerController controller;
    private float stepTimer;
    private float speed;

    [Header("Loudness Settings")]
    [SerializeField] private float minLoudness = 0.1f; // crouch quietness
    [SerializeField] private float maxLoudness = .4f; // sprint loudness

    private void Update()
    {
        if(_agent != null)
        {
            // Only horizontal speed matters for footsteps
            Vector3 horizontalVel = new Vector3(_agent.velocity.x, 0f, _agent.velocity.z);
            speed = horizontalVel.magnitude;
            bool isMoving = speed > 0.1f;

            if (isMoving)
            {
                stepTimer -= Time.deltaTime;

                // Scale step interval: faster speed shorter interval
                float scaledInterval = baseStepInterval * (baseSpeed / speed);

                if (stepTimer <= 0f)
                {
                    _soundEmitter.PlayOnce();
                    stepTimer = scaledInterval;
                }
            }
            else
            {
                stepTimer = 0f; // reset so next move plays immediately
            }
        }
        else
        {
            // Only horizontal speed matters for footsteps
            Vector3 horizontalVel = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
            speed = horizontalVel.magnitude;
            bool isMoving = speed > 0.1f;

            if (isMoving)
            {
                stepTimer -= Time.deltaTime;

                // Scale step interval: faster speed shorter interval
                float scaledInterval = baseStepInterval * (baseSpeed / speed);

                if (stepTimer <= 0f)
                {
                    PlayFootstep(scaledInterval);
                    stepTimer = scaledInterval;
                }
            }
            else
            {
                stepTimer = 0f; // reset so next move plays immediately
            }
        }

    }

    private void PlayFootstep(float scaledInterval)
    {
        float speedRatio = speed / baseSpeed;
        float loudness = Mathf.Lerp(minLoudness, maxLoudness, speedRatio);
        _soundEmitter?.PlayOnce(loudness, _agent == null);
    }
}
