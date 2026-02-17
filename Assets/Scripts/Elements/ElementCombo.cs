using UnityEngine;

[CreateAssetMenu(fileName = "New Element Combo", menuName = "Elements/Element Combo")]
public class ElementCombo : ScriptableObject
{
    public ElementPair inputElements;
    public Element resultElement;
    public float priority;
    public bool removeE1OldBehaviorsOnReaction;
    public bool removeE2OldBehaviorsOnReaction;

}