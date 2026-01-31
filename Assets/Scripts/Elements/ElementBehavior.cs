using System;
using UnityEngine;

[System.Serializable]
public struct ElementBehaviorContext
{
    public GameObject instigator;
    public GameObject target;
    public Vector3 position;
}

public abstract class ElementBehavior : ScriptableObject
{
    [TextArea]
    public string description;

    public float duration = 3f;

    public float defaultIntensity = 1f;
    public abstract Type StatusType { get; }

    public abstract void Apply(ElementBehaviorContext context);
}