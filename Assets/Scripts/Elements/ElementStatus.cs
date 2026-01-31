using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementStatus : MonoBehaviour
{
    public List<Element> currentElements = new();

    public void AddElement(Element element)
    {
        if (!currentElements.Contains(element))
        {
            currentElements.Add(element);
        }
        
        float maxDuration = 0f;
        foreach (var behavior in element.behaviors)
        {
            if (behavior.duration > maxDuration)
                maxDuration = behavior.duration;
        }
        StartCoroutine(RemoveElementAfterDelay(element, maxDuration));
    }

    public void RemoveElement(Element element)
    {
        if (currentElements.Contains(element))
        {
            currentElements.Remove(element);
        }
    }

    public IEnumerator RemoveElementAfterDelay(Element element, float delay)
    {
        yield return new WaitForSeconds(delay);
        RemoveElement(element);
    }
}