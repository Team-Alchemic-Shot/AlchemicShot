using UnityEngine;

public class BetaGunAdd : MonoBehaviour
{
    [SerializeField]
    private GameObject gunPrefab;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<Inventory>(out var inventory))
            {
                inventory.AddGun(gunPrefab);
                Destroy(gameObject); // destroy the pickup object after adding the gun
            }
        }
    }
}