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
        // TODO should this remove all elements with behaviors of this tag type?
        // this is called from Destroy, and also when Reactions destroys tags
        var elementsToRemove = currentElements
            .Where(e => e.behaviors.Any(b => b.TagType == tagType))
            .ToList();

        foreach (var element in elementsToRemove)
        {
            RemoveElement(element);
        }
    }

    public T GetBehavior<T>() where T : ElementBehavior
    {
        foreach (var element in currentElements)
        {
            var behavior = element.behaviors.OfType<T>().FirstOrDefault();
            if (behavior != null)
            {
                return behavior;
            }
        }
        return null;
    }

    public bool TryGetBehavior<T>(out T behavior) where T : ElementBehavior
    {
        behavior = GetBehavior<T>();
        return behavior != null;
    }

    public bool TryGetTag<T>(out T tag) where T : ElementTag
    {
        return TryGetComponent(out tag);
    }
}