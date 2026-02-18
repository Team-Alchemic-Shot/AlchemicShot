using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AoeTargeting
{
    /// <summary>
    /// Utility method to collect targets for an AOE effect, given a primary target, an origin point, a range, and a layer mask to filter valid targets.
    /// </summary>
    /// <param name="primary"></param>
    /// <param name="origin"></param>
    /// <param name="range"></param>
    /// <param name="mask"></param>
    /// <returns></returns>
    public static List<GameObject> CollectTargets(GameObject primary, Vector3 origin, float range, LayerMask mask)
    {
        var targets = new List<GameObject>();
        if (primary != null)
        {
            targets.Add(primary);
        }

        var hits = Physics.OverlapSphere(origin, range, mask);
        if (hits == null || hits.Length == 0)
        {
            return targets;
        }

        foreach (var hit in hits)
        {
            if (hit == null)
            {
                continue;
            }

            var target = hit.attachedRigidbody != null ? hit.attachedRigidbody.gameObject : hit.gameObject;
            if (target != null)
            {
                targets.Add(target);
            }
        }

        return targets.Where(t => t != null).Distinct().ToList();
    }

    /// <summary>
    /// Creates an ordered list of targets for an AOE effect, with the primary target first (if not null), 
    /// followed by the rest of the AOE targets in no particular order, excluding duplicates and nulls.
    /// </summary>
    /// <param name="primary"></param>
    /// <param name="aoeTargets"></param>
    /// <returns></returns>
    public static List<GameObject> CreateOrderedTargetList(GameObject primary, List<GameObject> aoeTargets)
    {
        var orderedTargets = new List<GameObject> { primary };
        orderedTargets.AddRange(aoeTargets.Where(t => t != null && t != primary));
        return orderedTargets;
    }
}
