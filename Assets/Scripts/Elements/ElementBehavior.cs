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

public abstract class ElementBehavior : ScriptableObject // no TOUCHY
{
    [TextArea]
    public string description;

    public float duration = 3f;

    public float defaultIntensity = 1f;
    public abstract Type TagType { get; }
    internal bool IsRuntimeInstance { get; private set; }

    /// <summary>
    /// Applies the behavior to the target in the given context.
    /// Called from the Gun Event system and after a reaction is triggered.
    /// </summary>
    /// <param name="context"></param>
    public abstract void Apply(ElementBehaviorContext context);

    /// <summary>
    /// The default removal logic for most behaviors.
    /// Removes the associated ElementTag from the target, if any.
    /// Also reverts any lasting effects via RevertEffects.
    /// </summary>
    /// <param name="context"></param>
    public virtual void Remove(ElementBehaviorContext context)
    {
        RevertEffects(context);
        if (TagType != null && context.target.TryGetComponent(TagType, out var tag))
        {
            Destroy(tag); // will remove from status automatically
        } else if (context.target.TryGetComponent<ElementStatus>(out var elementStatus))
        {
            elementStatus.RemoveElement(context.sourceBullet.element); // immediately remove instantaneous behaviors
            CancelRemoveBehavior(context); // don't need to double remove
        }
        if (IsRuntimeInstance)
        {
            Destroy(this); // clean up behavior instance
        }
    }

    /// <summary>
    /// Reverts any lasting effects applied by this behavior.
    /// Called upon removal of the behavior.
    /// </summary>
    /// <param name="context"></param>
    public virtual void RevertEffects(ElementBehaviorContext context)
    {
        // override in subclasses if effect has lasting impact beyond status
    }

    /// <summary>
    /// Hook for Gun Event system to apply all behaviors from a bullet's element to a target.
    /// </summary>
    /// <param name="context"></param>
    public static void ApplyBehaviors(ElementBehaviorContext context)
    {
        if (!context.target.TryGetComponent<ElementStatus>(out var elementStatus))
        {
            elementStatus = context.target.AddComponent<ElementStatus>();
        }
        elementStatus.AddElement(context.sourceBullet.element); // track applied element
        foreach (var behavior in context.sourceBullet.element.behaviors)
        {
            var behaviorInstance = Instantiate(behavior); // scriptableobjects stored on disk
            behaviorInstance.MarkRuntimeInstance();
            behaviorInstance.Apply(context);
        }
    }

    internal void MarkRuntimeInstance()
    {
        IsRuntimeInstance = true;
    }

    /// <summary>
    /// Schedules removal of this behavior from the target after a delay.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    private IEnumerator RemoveBehaviorAfterDelay(ElementBehaviorContext context, float delay)
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

    public void RemoveBehavior(ElementBehaviorContext context, float delay)
    {
        if (context.target.TryGetComponent<MonoBehaviour>(out var monoBehaviour))
        {
            monoBehaviour.StartCoroutine(RemoveBehaviorAfterDelay(context, delay));
        }
    }

    /// <summary>
    /// Cancels scheduled removal of this behavior from the target.
    /// Called when a Reaction occurs that removes this behavior before its scheduled removal.
    /// </summary>
    /// <param name="context"></param>
    public void CancelRemoveBehavior(ElementBehaviorContext context)
    {
        if (context.target.TryGetComponent<MonoBehaviour>(out var monoBehaviour))
        {
            monoBehaviour.StopCoroutine(RemoveBehaviorAfterDelay(context, duration));
        }
    }
}