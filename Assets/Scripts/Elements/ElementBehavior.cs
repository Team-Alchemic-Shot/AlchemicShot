using UnityEngine;

[System.Serializable]
public struct ElementBehaviorContext
{
    public GameObject instigator;
    public GameObject target;
    public Vector3 position;
    public float intensity;
}

public abstract class ElementBehavior : ScriptableObject
{
    [TextArea]
    public string description;

    public abstract void Apply(ElementBehaviorContext context);
}