using UnityEngine;

public class ZombieApplier : MonoBehaviour
{
    [SerializeField]
    private ZombieArchetype archetype;

    private void Awake()
    {
        if (archetype == null)
        {
            return;
        }

        if (TryGetComponent<ZombieTargeting>(out var targeting))
        {
            targeting.ApplyArchetype(archetype);
        }

        if (TryGetComponent<ZombieClimb>(out var climb))
        {
            climb.ApplyArchetype(archetype);
        }


        if (TryGetComponent<ZombieAttack>(out var attack))
        {
            attack.ApplyArchetype(archetype);
        }
    }
}