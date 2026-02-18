using System.Linq;
using UnityEngine;

public class ElementInventory : MonoBehaviour
{
    [SerializeField]
    private int slotCount = 14;

    [SerializeField]
    private ElementStack[] slots = new ElementStack[14];

    [SerializeField]
    private ElementDatabase elementDatabase;

    public int SlotCount => slots.Length;

    private void Awake()
    {
        EnsureSlots();
    }

    private void OnEnable()
    {
        // start with a random primitive element for testing purposes
        // could replace with a proper start screen menu for choosing a starting element
        GiveRandomPrimitive();
    }

    private void OnValidate()
    {
        if (slotCount < 1)
        {
            slotCount = 1;
        }

        EnsureSlots();
    }

    private void EnsureSlots()
    {
        if (slots == null)
        {
            slots = new ElementStack[slotCount];
        }
        else if (slots.Length != slotCount)
        {
            var resized = new ElementStack[slotCount];
            int copyLength = Mathf.Min(slots.Length, resized.Length);
            for (int i = 0; i < copyLength; i++)
            {
                resized[i] = slots[i];
            }
            slots = resized;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] ??= new ElementStack();
        }
    }

    public bool AddElement(Element element, int count)
    {
        if (element == null || count <= 0)
        {
            return false;
        }

        foreach (var stack in slots)
        {
            if (!stack.IsEmpty && stack.Element == element)
            {
                stack.Add(count);
                return true;
            }
        }

        foreach (var stack in slots)
        {
            if (stack.IsEmpty)
            {
                stack.Set(element, count);
                return true;
            }
        }

        return false;
    }

    public bool RemoveElement(Element element, int count)
    {
        if (element == null || count <= 0)
        {
            return false;
        }

        foreach (var stack in slots)
        {
            if (!stack.IsEmpty && stack.Element == element)
            {
                int removed = stack.Remove(count);
                return removed == count;
            }
        }

        return false;
    }

    public int GetCount(Element element)
    {
        if (element == null)
        {
            return 0;
        }

        foreach (var stack in slots)
        {
            if (!stack.IsEmpty && stack.Element == element)
            {
                return stack.Count;
            }
        }

        return 0;
    }

    public ElementStack GetSlot(int index)
    {
        if (slots == null || index < 0 || index >= slots.Length)
        {
            return null;
        }

        return slots[index];
    }

    public void GiveRandomPrimitive()
    {
        var primitiveElements = elementDatabase.elements
            .Where(e => e.elementTier == ElementTier.Primitive).ToArray();
        if (primitiveElements.Length == 0)        {
            Debug.LogWarning("No primitive elements found in database.");
            return;
        }
        var randomElement = primitiveElements[Random.Range(0, primitiveElements.Length)];
        AddElement(randomElement, 1);
    }
}