using System.Linq;
using UnityEngine;

public class BetaElementAdder : MonoBehaviour
{
    [SerializeField]
    private Element elementToAdd;
    [SerializeField]
    private int elementCount = 1;
    [SerializeField]
    private bool randomElement = true;

    [SerializeField]
    private ElementTier tier = ElementTier.Primitive;
    [SerializeField]
    private ElementDatabase elementDatabase;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<ElementInventory>(out var inventory))
            {
                if (randomElement)
                {
                    var elementsOfTier = elementDatabase.elements.Where(e => e.elementTier == tier).ToList();
                    if (elementsOfTier.Count == 0)
                    {
                        Debug.LogWarning($"No elements of tier {tier} found in database.");
                        return;
                    }
                    elementToAdd = elementsOfTier[Random.Range(0, elementsOfTier.Count)];
                }
                inventory.AddElement(elementToAdd, elementCount);
                Destroy(gameObject); // destroy the pickup object after adding the element
            }
        }
    }
}