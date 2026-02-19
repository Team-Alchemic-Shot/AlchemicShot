using UnityEngine;

public class BetaElementAdder : MonoBehaviour
{
    [SerializeField]
    private Element elementToAdd;
    [SerializeField]
    private int elementCount = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<ElementInventory>(out var inventory))
            {
                inventory.AddElement(elementToAdd, elementCount);
                Destroy(gameObject); // destroy the pickup object after adding the element
            }
        }
    }
}