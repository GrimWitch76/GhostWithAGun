using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class GhostMovement : MonoBehaviour
{
    [SerializeField] private float _turnSpeed = 2f;
    [SerializeField] private float roamRadius = 10f;
    [SerializeField] private float roamDelay = 3f;

    private NavMeshAgent agent;
    private GhostBrain _brain;
    private float roamTimer;
    private bool _wander;

    void Awake() => agent = GetComponent<NavMeshAgent>();
    void OnEnable() => _brain = GetComponent<GhostBrain>();

    public bool AtDestination() =>
        !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;

    void Update()
    {
        if (_wander)
        {
            roamTimer += Time.deltaTime;

            if (roamTimer >= roamDelay && !agent.pathPending && agent.remainingDistance < 0.5f)
            {
                agent.isStopped = false;
                Vector3 newPos = RandomNavSphere(transform.position, roamRadius, NavMesh.AllAreas);
                SetDestinationSafe(newPos);
                roamTimer = 0;
            }
        }
    }

    public void StartWander() => _wander = true;

    public void MoveToPoint(Vector3 pt)
    {
        _wander = false;
        agent.isStopped = false;
        roamTimer = 0;
        SetDestinationSafe(pt);
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

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMesh.SamplePosition(randDirection, out NavMeshHit navHit, dist, layermask);
        return navHit.position;
    }

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
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * _turnSpeed);
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
        if (!door.IsOpen)
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

        if (door.IsOpen) yield break;

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
    //    agent.isStopped = true;
    //    LookAt(door.transform.position);

    //    // Fire until it opens (lock/barricade breaks)
    //    while (!door.IsOpen && door.IsBarricaded))
    //    {
    //        if (_brain.Gun.CanFire)
    //        {
    //            _brain.Gun.Fire(transform.position + Vector3.up * 1.5f, door.transform.position);
    //            if (door.IsBarricaded) door.BreakBarricade();
    //        }
    //        else
    //        {
    //            _brain.Gun.Reload();
    //        }
    //        yield return null;
    //    }

    //    agent.isStopped = false;
    }
}