using System.Collections.Generic;
using UnityEngine;

public class ElementStatus : MonoBehaviour
{
    public List<Element> currentElements = new();
    private readonly Dictionary<Element, List<ElementBehavior>> activeBehaviors = new(); // tracks active behavior instances for each element, to know when to remove the element and to clean up old behaviors on reaction

    public void AddElement(Element element)
    {
        currentElements.Add(element);
        // TODO (design): decide how to represent repeated applications of the same element.
        // Today we append duplicates and use order for reactions (last two elements), which can
        // leave multiple identical entries if the same element hits repeatedly. Consider:
        // - switching to a multiset/count per element for stacking behavior,
        // - de-duplicating while still tracking "last applied" for reactions,
        // - or attaching timestamps/sequence IDs to keep reaction order without duplicates.
        // This decision also affects how durations refresh/stack and how RemoveElement should behave.
    }

    public void RemoveElement(Element element)
    {
        if (element == null || currentElements.Count == 0)
        {
            return;
        }

        while (currentElements.Remove(element))
        {
            // remove all occurrences to fully clear the status
        }
    }

    /// <summary>
    /// Register a behavior instance for an element.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="behavior"></param>
    public void RegisterBehaviorInstance(Element element, ElementBehavior behavior)
    {
        if (element == null || behavior == null)
        {
            return;
        }

        if (!activeBehaviors.TryGetValue(element, out var behaviors))
        {
            behaviors = new List<ElementBehavior>();
            activeBehaviors[element] = behaviors;
        }

        behaviors.Add(behavior);
    }

    /// <summary>
    /// Unregister a behavior instance for an element. Called when a behavior is removed, to keep track of active behaviors and remove the element when no behaviors remain.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="behavior"></param>
    public void UnregisterBehaviorInstance(Element element, ElementBehavior behavior)
    {
        if (element == null || behavior == null)
        {
            return;
        }

        if (!activeBehaviors.TryGetValue(element, out var behaviors))
        {
            return;
        }

        behaviors.Remove(behavior);
        if (behaviors.Count == 0)
        {
            activeBehaviors.Remove(element);
            RemoveElement(element);
        }
    }

    /// <summary>
    /// Removes all behaviors for an element. Called when an element is removed or a reaction causes an element to be replaced, to clean up old behaviors and optionally remove the element if no behaviors remain.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="context"></param>
    public void RemoveBehaviorsForElement(Element element, ElementBehaviorContext context)
    {
        if (element == null)
        {
            return;
        }

        if (!activeBehaviors.TryGetValue(element, out var behaviors) || behaviors.Count == 0)
        {
            RemoveElement(element);
            return;
        }

        var behaviorsToRemove = behaviors.ToArray();
        foreach (var behavior in behaviorsToRemove)
        {
            if (behavior != null)
            {
                behavior.Remove(context);
            }
        }
    }

    /// <summary>
    /// Try to get a tag of a specific type from the target. Used by behaviors to check for existing tags and apply tag effects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tag"></param>
    /// <returns></returns>
    public bool TryGetTag<T>(out T tag) where T : ElementTag
    {
        return TryGetComponent(out tag);
    }


    /// <summary>
    /// Utility method to get or create an ElementStatus component on a target GameObject. 
    /// Used by behaviors and reactions to ensure the target has an ElementStatus for tracking elements and behaviors.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public static ElementStatus GetOrCreateElementStatus(GameObject target)
    {
        if (target == null)
        {
            return null;
        }

        if (!target.TryGetComponent<ElementStatus>(out var elementStatus))
        {
            elementStatus = target.AddComponent<ElementStatus>();
        }

        return elementStatus;
    }
}