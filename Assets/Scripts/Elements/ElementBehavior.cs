using System;
using System.Collections;
using UnityEngine;

[Serializable]
public struct ElementBehaviorContext
{
    public GameObject instigator;
    public GameObject target;
    public Vector3 position;
    public BulletData sourceBullet;
}

public abstract class ElementBehavior : ScriptableObject
{
    [TextArea]
    public string description;

    public float duration = 3f;

    public float defaultIntensity = 1f;
    public abstract Type TagType { get; }

    public abstract void Apply(ElementBehaviorContext context);

    public virtual void Remove(ElementBehaviorContext context) // NO TOUCHY
    {
        if (context.target.TryGetComponent<ElementStatus>(out var elementStatus))
        {
            elementStatus.RemoveElement(context.sourceBullet.element);
        }
        if (TagType != null && context.target.TryGetComponent(TagType, out var tag))
        {
            Destroy(tag);
        }
        RevertEffects(context);
    }

    public virtual void RevertEffects(ElementBehaviorContext context)
    {
        // override in subclasses if effect has lasting impact beyond status
    }

    public static void ApplyBehaviors(ElementBehaviorContext context)
    {
        if (!context.target.TryGetComponent<ElementStatus>(out var elementStatus))
        {
            elementStatus = context.target.AddComponent<ElementStatus>();
        }
        elementStatus.AddElement(context.sourceBullet.element); // track applied element
        foreach (var behavior in context.sourceBullet.element.behaviors)
        {
            behavior.Apply(context);
        }
    }

    public IEnumerator RemoveBehaviorAfterDelay(ElementBehaviorContext context, float delay)
    {
        yield return new WaitForSeconds(delay);
        Remove(context);
    }

    /// <summary>
    /// Schedules removal of this behavior from the target after a delay.
    /// IMPORTANT: Call this if the behavior is instantaneous to ensure it gets removed after a grace period for reactions.
    /// </summary>
    /// <param name="context"></param>
    public void RemoveBehavior(ElementBehaviorContext context)
    {
        if (context.target.TryGetComponent<MonoBehaviour>(out var monoBehaviour))
        {
            monoBehaviour.StartCoroutine(RemoveBehaviorAfterDelay(context, duration));
        }
    }
}