using UnityEngine;

/// <summary>
/// A collection of elements and their counts, representing a stack of elements in the player's inventory.
/// </summary>
[System.Serializable]
public class ElementStack
{
    public Element Element { get; private set; }
    public int Count { get; private set; }

    public bool IsEmpty => Element == null || Count <= 0;

    public ElementStack() : this(null, 0)
    {
    }

    public ElementStack(Element element, int count)
    {
        Element = element;
        Count = count;
        if (Count <= 0)
        {
            Element = null;
            Count = 0;
        }
    }

    public void Set(Element newElement, int newCount)
    {
        Element = newElement;
        Count = newCount;
        if (Count <= 0)
        {
            Element = null;
            Count = 0;
        }
    }

    public void Add(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (Element == null)
        {
            return;
        }

        Count += amount;
    }

    public int Remove(int amount)
    {
        if (amount <= 0 || IsEmpty)
        {
            return 0;
        }

        int removed = Mathf.Min(amount, Count);
        Count -= removed;
        if (Count <= 0)
        {
            Element = null;
            Count = 0;
        }

        return removed;
    }

    public void Clear()
    {
        Element = null;
        Count = 0;
    }
}