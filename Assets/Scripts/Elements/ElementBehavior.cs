using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Struct to pass relevant context information to behaviors when applying or removing them, to allow for more complex and reactive behavior logic based on the context of application and removal.
/// </summary>
[Serializable]
public class ElementBehaviorContext
{
    public GameObject instigator;
    public GameObject Target => targets.Count > 0 ? targets.First() : null;
    public Queue<GameObject> targets;
    public Vector3 position;
    public BulletData sourceBullet;
}

/// <summary>
/// Base class for behaviors applied by elements to targets, which can apply lasting effects and track tags on the target and be removed after a duration or by reactions. 
/// Behaviors are applied from the Gun Event system and from reactions, and can be applied to any target with an ElementStatus component (added dynamically).
/// </summary>
public abstract class ElementBehavior : ScriptableObject // no TOUCHY
{
    [TextArea]
    public string description;

    public float duration = 3f;

    public float defaultIntensity = 1f;
    internal bool IsRuntimeInstance { get; private set; }
    internal Element OwnerElement { get; private set; } // element instance that owns this behavior instance, for tracking in the ElementStatus and cleanup on removal
    private readonly List<ElementTag> ownedTags = new(); // tracks tags applied by this behavior instance, to clean up on removal

    /// <summary>
    /// Applies the behavior to the target in the given context.
    /// Called from the Gun Event system and after a reaction is triggered.
    /// </summary>
    /// <param name="context"></param>
    public abstract void Apply(ElementBehaviorContext context);

    /// <summary>
    /// The default removal logic for most behaviors.
    /// Removes any tags tracked by this behavior instance, if any.
    /// Also reverts any lasting effects via RevertEffects.
    /// </summary>
    /// <param name="context"></param>
    public virtual void Remove(ElementBehaviorContext context)
    {
        RevertEffects(context);
        RemoveOwnedTags();
        if (context.Target != null && context.Target.TryGetComponent<ElementStatus>(out var elementStatus))
        {
            elementStatus.UnregisterBehaviorInstance(OwnerElement, this);
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
        if (context.sourceBullet.element == null || context.sourceBullet.element.behaviors == null)
        {
            return;
        }
        var elementStatus = ElementStatus.GetOrCreateElementStatus(context.Target);
        elementStatus.AddElement(context.sourceBullet.element); // track applied element
        foreach (var behavior in context.sourceBullet.element.behaviors)
        {
            var behaviorInstance = CreateRuntimeBehavior(behavior, context, elementStatus);
            behaviorInstance.Apply(context);
        }
    }

    /// <summary>
    /// Marks this behavior instance as a runtime instance, which will allow it to be cleaned up when removed.
    /// </summary>
    internal void MarkRuntimeInstance()
    {
        IsRuntimeInstance = true;
    }

    /// <summary>
    /// Sets the element that owns this behavior instance, for tracking in the ElementStatus and cleanup on removal.
    /// </summary>
    /// <param name="element"></param>
    internal void SetOwnerElement(Element element)
    {
        OwnerElement = element;
    }

    /// <summary>
    /// Tracks a tag instance as being owned by this behavior, so it can be removed when the behavior is removed.
    /// </summary>
    /// <param name="tag"></param>
    protected void TrackTag(ElementTag tag, ElementBehaviorContext context)
    {
        if (tag == null)
        {
            return;
        }

        if (!ownedTags.Contains(tag))
        {
            ownedTags.Add(tag);
        }

        tag.AddOwner(this, context);
    }

    /// <summary>
    /// Destroys all tags tracked as owned by this behavior instance.
    /// </summary>
    private void RemoveOwnedTags()
    {
        if (ownedTags.Count == 0)
        {
            return;
        }

        for (var i = ownedTags.Count - 1; i >= 0; i--)
        {
            var tag = ownedTags[i];
            if (tag != null)
            {
                tag.RemoveOwner(this);
            }
            ownedTags.RemoveAt(i);
        }
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
        RemoveBehavior(context, duration);
    }

    /// <summary>
    /// Schedules removal of this behavior from the target after a delay.
    /// IMPORTANT: Call this if the behavior is instantaneous to ensure it gets removed after a grace period for reactions.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="delay"></param>
    public void RemoveBehavior(ElementBehaviorContext context, float delay)
    {
        if (context.Target.TryGetComponent<MonoBehaviour>(out var monoBehaviour))
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
        if (context.Target.TryGetComponent<MonoBehaviour>(out var monoBehaviour))
        {
            monoBehaviour.StopCoroutine(RemoveBehaviorAfterDelay(context, duration));
        }
    }

    /// <summary>
    /// Utility method to create a runtime instance of a behavior for application to a target, 
    /// with proper registration in the ElementStatus, ownership, and flagging for tracking and cleanup on removal.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="behavior"></param>
    /// <param name="targetContext"></param>
    /// <param name="elementStatus"></param>
    /// <returns></returns>
    public static T CreateRuntimeBehavior<T>(
        T behavior, 
        ElementBehaviorContext targetContext, 
        ElementStatus elementStatus) where T : ElementBehavior
    {
        var behaviorInstance = GameObject.Instantiate(behavior);
        behaviorInstance.SetOwnerElement(targetContext.sourceBullet.element);
        behaviorInstance.MarkRuntimeInstance();
        elementStatus.RegisterBehaviorInstance(targetContext.sourceBullet.element, behaviorInstance);
        return behaviorInstance;
    }
}