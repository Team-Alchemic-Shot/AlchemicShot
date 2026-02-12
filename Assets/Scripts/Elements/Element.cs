using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Element", menuName = "Elements/Element")]
public class Element : ScriptableObject
{   
    public string elementName;
    public Color elementColor;
    public float elementDensity;
    public Sprite elementIcon;
    public ElementTier elementTier;
    public GameObject vfxPrefab;
    public AudioClip sfxClip;
    public float sfxVolume = 1.0f;

    public List<ElementBehavior> behaviors = new();
}

[System.Serializable]
public enum ElementTier
{
    Primitive,
    Tier1,
    Tier2
}
