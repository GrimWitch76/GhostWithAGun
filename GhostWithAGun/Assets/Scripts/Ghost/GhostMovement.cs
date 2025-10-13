using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class GhostMovement : MonoBehaviour
{


    private NavMeshAgent agent;
    private GhostBrain _brain;
    private float roamTimer;
    private bool _wander;

    private List<RoomAnchor> _rooms = new();
    void OnEnable() => _brain = GetComponent<GhostBrain>();

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        _rooms = FindObjectsByType<RoomAnchor>(FindObjectsSortMode.None).ToList();
        Debug.Log("Ghost found "+ _rooms.Count.ToString() + " room anchors");
    }

    public bool AtDestination() =>
        !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;

    void Update()
    {
        if (_wander)
        {
            roamTimer += Time.deltaTime;
            if (roamTimer >= _brain.Tuning.roamDelay && !agent.pathPending && agent.remainingDistance < 0.5f)
            {
                Vector3 newPos = PickSmartWanderPoint();
                agent.SetDestination(newPos);
                roamTimer = 0f;
            }
        }
    }

    private Vector3 PickSmartWanderPoint()
    {
        if (_rooms == null || _rooms.Count == 0)
            return RandomNavSphere(transform.position, 10f, NavMesh.AllAreas);

        // Occasionally move to a *different* room to cover more ground
        RoomAnchor targetRoom;
        if (Random.value < _brain.Tuning.longRangeChance)
        {
            // Weighted pick based on room weights
            float totalWeight = _rooms.Sum(r => r.weight);
            float pick = Random.value * totalWeight;
            float cumulative = 0;
            targetRoom = _rooms.FirstOrDefault(r => (cumulative += r.weight) >= pick);
        }
        else
        {
            // Otherwise pick nearest 2–3 rooms and pick one of those
            var nearest = _rooms
                .OrderBy(r => Vector3.Distance(transform.position, r.transform.position))
                .Take(3)
                .ToList();
            targetRoom = nearest[Random.Range(0, nearest.Count)];
        }

        Vector3 chosenCenter = targetRoom.transform.position;

        // Small offset around the center so it doesn’t stand exactly on the marker
        Vector3 randomOffset = Random.insideUnitSphere * _brain.Tuning.roomCenterBiasRadius;
        randomOffset.y = 0;

        Vector3 candidate = chosenCenter + randomOffset;

        // Snap to NavMesh
        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            return hit.position;

        return chosenCenter;
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMesh.SamplePosition(randDirection, out NavMeshHit navHit, dist, layermask);
        return navHit.position;
    }
    public void MoveToPoint(Vector3 pt)
    {
        _wander = false;
        agent.isStopped = false;
        roamTimer = 0;
        SetDestinationSafe(pt);
    }

    public Vector3 GetDestination()
    {
        if(agent != null)
        {
            return agent.destination;
        }
        return Vector3.zero;
    }

    private void SetDestinationSafe(Vector3 dst)
    {
        // Prefer completed paths; otherwise still set but we can react to partial
        var path = new NavMeshPath();
        agent.CalculatePath(dst, path);
        agent.SetPath(path);

        if (path.status == NavMeshPathStatus.PathPartial)
        {
            // Optional: nudge intermediate points or try nearby samples
            // In practice, the brain will detect blockage near doors and escalate.
        }
    }
    public void StartWander() => _wander = true;

    public void Stop() => agent.isStopped = true;

    public void LookAt(Vector3 target)
    {
        Vector3 dir = (target - transform.position); dir.y = 0;
        if (dir.sqrMagnitude > 1e-4f)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
        }
    }

    public void LookRandomDirection()
    {
        Vector3 randomDir = Random.insideUnitSphere; randomDir.y = 0;
        Quaternion lookRot = Quaternion.LookRotation(randomDir);
        StartCoroutine(RotateSmoothly(lookRot));
    }

    private IEnumerator RotateSmoothly(Quaternion target)
    {
        while (Quaternion.Angle(transform.rotation, target) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * _brain.Tuning._turnSpeed);
            yield return null;
        }
        transform.rotation = target;
    }

    // === Search helpers ===

    public void SearchNearby(Vector3 center, float radius, int count)
    {
        if (center == Vector3.zero) return;

        for (int i = 0; i < count; i++)
        {
            Vector3 p = center + Random.insideUnitSphere * radius;
            p.y = center.y;
            if (NavUtil.CanReach(agent, p))
            {
                MoveToPoint(p);
                return;
            }
        }
        // fallback: just rotate
        LookRandomDirection();
    }

    // === Door handling (trigger) ===

    private void OnTriggerEnter(Collider other)
    {
        var door = other.GetComponent<Door>();
        if (door == null) return;

        // Don’t care about closing; only opening/breaking if it blocks us.
        if (!door.IsOpen && _brain.HasGun)
            StartCoroutine(HandleDoor(door));
    }

    private IEnumerator HandleDoor(Door door)
    {
        // First try open
        door.GhostInteract(gameObject);
        // Give it a tiny moment to respond
        float t = 0f;
        while (!door.IsOpen && t < 0.5f)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (!door.IsBarricaded) yield break;

        // If still blocked: decide to break or path around
        bool canReachAround = false;
        {
            // quick probe: can we reach our destination anyway?
            if (agent.hasPath && agent.path.corners.Length > 0)
                canReachAround = agent.path.status == NavMeshPathStatus.PathComplete;
        }

        if (!canReachAround)
        {
            // If we have (or will have) a gun, break through
            // Gate on suspicion/frustration via the brain's thresholds
            if (_brain.HasGun || _brain.Suspicion >= _brain.Tuning.breakDoorSuspicionGate /*expose if needed*/)
            {
                yield return BreakDoorRoutine(door);
            }
            else
            {
                _brain.ReArm();
            }
        }
    }

    private IEnumerator BreakDoorRoutine(Door door)
    {
        yield return null;

        agent.isStopped = true;
        LookAt(door.transform.position);
        door.Shatter();

        // Fire until it opens (lock/barricade breaks)
        while (!door.IsOpen && door.IsBarricaded)
        {
            if (_brain.Gun.CanFire)
            {
                _brain.Gun.Fire(transform.position + Vector3.up * 1.5f, door.transform.position);

                if (door.IsBarricaded)
                    door.Shatter();
            }
            else
            {
                _brain.Gun.Reload();
            }

            yield return null;
        }

        // Resume movement after door is open
        agent.isStopped = false;
    }
}