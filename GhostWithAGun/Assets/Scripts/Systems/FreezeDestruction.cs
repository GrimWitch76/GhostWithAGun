using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FreezeSettledChunks : MonoBehaviour
{
    [SerializeField] private float checkInterval = 1f;   // How often to check chunks
    [SerializeField] private float sleepThreshold = 0.05f; // Velocity threshold to consider “settled”
    [SerializeField] private float settleTime = 2f;      // How long it must stay below threshold before freezing

    private float timer;
    private List<Rigidbody> _childRb;
    private Dictionary<Rigidbody, float> _restTimers = new();

    private void Start()
    {
        _childRb = GetComponentsInChildren<Rigidbody>().ToList();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < checkInterval) return;
        timer = 0f;

        var toFreeze = new List<Rigidbody>();

        foreach (var rb in _childRb)
        {
            if (rb == null) continue; // Safety check if something got destroyed externally

            bool isResting = rb.IsSleeping() || rb.linearVelocity.sqrMagnitude < sleepThreshold * sleepThreshold;

            if (isResting)
            {
                if (!_restTimers.ContainsKey(rb))
                    _restTimers[rb] = 0f;

                _restTimers[rb] += checkInterval;

                if (_restTimers[rb] >= settleTime)
                    toFreeze.Add(rb);
            }
            else
            {
                // Reset timer if it moved again
                if (_restTimers.ContainsKey(rb))
                    _restTimers[rb] = 0f;
            }
        }

        // Process all freezes after iteration
        foreach (var rb in toFreeze)
        {
            FreezeChunk(rb);
        }
    }

    private void FreezeChunk(Rigidbody rb)
    {
        _childRb.Remove(rb);
        _restTimers.Remove(rb);

        Destroy(rb);
        rb.gameObject.isStatic = true;
    }
}
