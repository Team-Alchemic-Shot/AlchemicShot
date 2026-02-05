using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElementStatus : MonoBehaviour
{
    public List<Element> currentElements = new();

    public void AddElement(Element element)
    {
        currentElements.Add(element);
        // TODO maybe should refresh duration or stack intensity of existing element behaviors?
    }

    public void RemoveElement(Element element)
    {
        if (currentElements.Contains(element))
        {
            currentElements.Remove(element);
        }
    }

    public void RemoveElementFromTag(Type tagType)
    {
        var elementsToRemove = currentElements
            .Where(e => e.behaviors.Any(b => b.TagType == tagType))
            .ToList();

        foreach (var element in elementsToRemove)
        {
            RemoveElement(element);
        }
    }
}