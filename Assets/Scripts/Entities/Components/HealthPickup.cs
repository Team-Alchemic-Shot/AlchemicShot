using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Heal Settings")]
    public int healAmount = 25;

    [Header("Pickup Settings")]
    public bool destroyOnPickup = true;

    private void OnTriggerEnter(Collider other)
    {
        var health = other.GetComponent<Health>();
        if (health == null)
        {
            return;
        }

        health.Heal(healAmount);

        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
    }
}