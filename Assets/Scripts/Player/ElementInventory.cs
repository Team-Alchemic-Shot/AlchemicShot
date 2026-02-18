using UnityEngine;

public class ElementInventory : MonoBehaviour
{
    [SerializeField]
    private int slotCount = 14;

    [SerializeField]
    private ElementStack[] slots = new ElementStack[14];

    public int SlotCount => slots.Length;

    private void Awake()
    {
        EnsureSlots();
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
}