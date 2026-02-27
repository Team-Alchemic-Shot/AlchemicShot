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
        if (healthPickupPrefab == null || Random.value > dropChance)
        {
            return;
        }

        Vector3 spawnPos = transform.position;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100f))
        {
            spawnPos = hit.point + Vector3.up * 0.15f;
        }

        Instantiate(healthPickupPrefab, spawnPos, Quaternion.identity);
    }
}