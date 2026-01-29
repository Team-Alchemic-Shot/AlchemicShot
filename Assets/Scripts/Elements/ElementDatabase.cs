using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Element Database", menuName = "Elements/Element Database")]
public class ElementDatabase : ScriptableObject
{
    public Element[] elements;
    public ElementCombo[] elementCombos;
    private Dictionary<ElementPair, Element> comboLookup;

    private void OnEnable()
    {
        comboLookup = new Dictionary<ElementPair, Element>();
        foreach (var combo in elementCombos)
        {
            comboLookup[new ElementPair(combo.inputElements.elementA, combo.inputElements.elementB)] = combo.resultElement;
            comboLookup[new ElementPair(combo.inputElements.elementB, combo.inputElements.elementA)] = combo.resultElement;
        }
    }

    public Element GetComboResult(Element a, Element b)
    {
        if (comboLookup.TryGetValue(new ElementPair(a, b), out Element result))
        {
            return result;
        }
        return null;
    }

    public Element GetElementByName(string name)
    {
        foreach (var element in elements)
        {
            if (element.elementName == name)
            {
                return element;
            }
        }
        return null;
    }
}

[System.Serializable]
public struct ElementPair
{
    public Element elementA;
    public Element elementB;

    public ElementPair(Element a, Element b)
    {
        elementA = a;
        elementB = b;
    }
}