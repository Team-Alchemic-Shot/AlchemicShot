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

    public float defaultIntensity = 1f;

    public abstract void Apply(ElementBehaviorContext context);
}