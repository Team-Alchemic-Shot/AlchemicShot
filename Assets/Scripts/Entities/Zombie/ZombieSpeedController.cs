using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieSpeedController : MonoBehaviour
{
    private NavMeshAgent agent;
    private ZombieTargeting targeting;

    // systems (elements/tags) should never directly set agent.speed.
    // register modifiers here 
    private readonly Dictionary<object, float> speedMultipliers = new();
    private readonly HashSet<object> freezes = new();

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        targeting = GetComponent<ZombieTargeting>();
    }

    private void LateUpdate()
    {
        if (agent == null || !agent.isOnNavMesh)
        {
            return;
        }

        // frozen freezes
        if (IsFrozen())
        {
            if (!agent.isStopped)
            {
                agent.isStopped = true;
            }

            agent.velocity = Vector3.zero;
            agent.speed = 0f;
            return;
        }

        if (agent.isStopped)
        {
            agent.isStopped = false;
        }

        // ai gives the base speed. status effects apply multipliers.
        var desiredSpeed = targeting != null ? targeting.DesiredSpeed : agent.speed;
        var multiplier = GetTotalMultiplier();

        agent.speed = Mathf.Max(0f, desiredSpeed * multiplier);
    }

    public void SetSpeedMultiplier(object source, float multiplier)
    {
        if (source == null)
        {
            return;
        }

        speedMultipliers[source] = Mathf.Max(0f, multiplier);
    }

    public void ClearSpeedMultiplier(object source)
    {
        if (source == null)
        {
            return;
        }

        speedMultipliers.Remove(source);
    }

    public void AddFreeze(object source)
    {
        if (source == null)
        {
            return;
        }

        freezes.Add(source);
    }

    public void RemoveFreeze(object source)
    {
        if (source == null)
        {
            return;
        }

        freezes.Remove(source);
    }

    private bool IsFrozen()
    {
        return freezes.Count > 0;
    }

    private float GetTotalMultiplier()
    {
        float total = 1f;

        foreach (var kvp in speedMultipliers)
        {
            total *= kvp.Value;
        }

        
        if (total < 0f)
        {
            total = 0f;
        }

        return total;
    }
}