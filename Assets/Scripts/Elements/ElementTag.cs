using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for tags applied by element behaviors to the target, to track and apply tag effects and notify owner behaviors of tag removal.
/// </summary>
public abstract class ElementTag : MonoBehaviour
{
    /// <summary>
    /// Struct to track the owner behavior and context for each tag instance, 
    /// to know which behaviors to notify when the tag is removed and to avoid collisions between multiple tags of the same type from different behaviors. 
    /// Stored as a list in the tag instance, since multiple behaviors can apply the same tag type with different contexts.
    /// </summary>
    private readonly struct OwnerEntry
    {
        public readonly ElementBehavior Behavior;
        public readonly ElementBehaviorContext Context;

        public OwnerEntry(ElementBehavior behavior, ElementBehaviorContext context)
        {
            Behavior = behavior;
            Context = context;
        }
    }

    private readonly List<OwnerEntry> owners = new();

    /// <summary>
    /// Adds an owner behavior and context to this tag instance. Called by behaviors when applying a tag, to keep track of which behaviors applied this tag and should be notified when it is removed.
    /// </summary>
    /// <param name="behavior"></param>
    /// <param name="context"></param>
    internal void AddOwner(ElementBehavior behavior, ElementBehaviorContext context)
    {
        if (behavior == null)
        {
            return;
        }
        for (var i = 0; i < owners.Count; i++)
        {
            if (owners[i].Behavior == behavior)
            {
                return;
            }
        }
        owners.Add(new OwnerEntry(behavior, context));
    }

    internal void RemoveOwner(ElementBehavior behavior, bool destroyIfEmpty = true)
    {
        if (behavior == null || owners.Count == 0)
        {
            return;
        }

        for (var i = owners.Count - 1; i >= 0; i--)
        {
            if (owners[i].Behavior == behavior)
            {
                owners.RemoveAt(i);
            }
        }

        if (destroyIfEmpty && owners.Count == 0)
        {
            Destroy(this);
        }
    }

    /// <summary>
    /// Applies the tag effects with the given parameters. Called by behaviors when applying the tag, to apply the tag effects based on the behavior's parameters and context. 
    /// The tag instance should use the provided parameters to apply its effects, and rely on the owner behavior to call RemoveOwners when the tag is removed, to clean up any lasting effects and notify the owner behavior of the removal via its context.
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="interval"></param>
    /// <param name="intensity"></param>
    /// <param name="refreshDuration"></param>
    /// <param name="stackIntensity"></param>
    /// <param name="instigator"></param>
    /// <param name="logTicks"></param>
    public abstract void Apply(
            float duration, 
            float interval, 
            float intensity, 
            bool refreshDuration, 
            bool stackIntensity,
            GameObject instigator, 
            bool logTicks);

    /// <summary>
    /// Removes the tag effects and notifies owner behaviors of the removal. Called by owner behaviors when removing the tag, to clean up any lasting effects and notify the owner behaviors of the removal via
    /// their contexts.
    /// </summary>
    protected void RemoveOwners()
    {
        if (owners.Count == 0)
        {
            Destroy(this);
            return;
        }

        var ownerSnapshot = owners.ToArray();
        owners.Clear();
        foreach (var owner in ownerSnapshot)
        {
            if (owner.Behavior != null)
            {
                owner.Behavior.Remove(owner.Context);
            }
        }

        if (owners.Count == 0)
        {
            Destroy(this);
        }
    }

    protected virtual void OnDestroy()
    {
        // ElementStatus removal is handled by behavior instances to avoid tag-type collisions.
    }

    public static T GetOrAddTag<T>(GameObject target) where T : ElementTag
    {
        if (target == null)
        {
            return null;
        }

        if (!target.TryGetComponent<T>(out var tag))
        {
            tag = target.AddComponent<T>();
        }
        return tag;
    }
}