using UnityEngine;
using UnityEngine.AI;

public static class NavUtil
{
    // fast “can I reach?” probe
    public static bool CanReach(NavMeshAgent agent, Vector3 dst)
    {
        var path = new NavMeshPath();
        agent.CalculatePath(dst, path);
        return path.status == NavMeshPathStatus.PathComplete;
    }
}
