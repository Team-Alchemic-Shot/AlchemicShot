using UnityEngine;

public class ZombieComponent : EntityComponent
{
    protected override void Awake()
    {
        var mgr = FindAnyObjectByType<ZombieRoundManager>();
        if (mgr == null)
        {
            Debug.LogWarning("No ZombieRoundManager found in the scene. Zombie deaths will not be tracked.");
            return;
        }

        if (TryGetComponent(out Health health))
        {
            health.OnDeath += (health) => mgr.OnZombieDied(health);
        }
    }
}