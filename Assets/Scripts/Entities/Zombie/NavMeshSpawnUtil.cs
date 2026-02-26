using UnityEngine;
using UnityEngine.AI;

public static class NavMeshSpawnUtil
{
    // Utility for spawn systems: ensures a spawned object with a NavMeshAgent ends up on a valid navmesh surface.
    // Returns true if the agent is on the navmesh after the operation 


    public static bool PlaceOnNavMesh(GameObject go, float maxDistance = 3f)
    {
        if (go == null)
        {
            return false;
        }

        var agent = go.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            return false;
        }

        
        if (agent.isOnNavMesh)
        {
            return true;
        }

        // Sample a nearby point on the navmesh and warp the agent there.
        if (NavMesh.SamplePosition(go.transform.position, out var hit, maxDistance, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            return agent.isOnNavMesh;
        }

        return false;
    }
}