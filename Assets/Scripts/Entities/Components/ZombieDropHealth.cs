using UnityEngine;

[DisallowMultipleComponent]
public class ZombieDropHealth : MonoBehaviour
{
    [Header("Drop")]
    [SerializeField] private GameObject healthPickupPrefab;
    [SerializeField, Range(0f, 1f)] private float dropChance = 0.15f;

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
        if (health != null)
        {
            health.OnDeath += HandleDeath;
        }
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnDeath -= HandleDeath;
        }
    }

    private void HandleDeath(Health deadHealth)
    {
        if (healthPickupPrefab == null)
        {
            return;
        }

        if (Random.value > dropChance)
        {
            return;
        }

        Instantiate(healthPickupPrefab, transform.position, Quaternion.identity);
    }
}