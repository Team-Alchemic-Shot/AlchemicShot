using Unity.VisualScripting;
using UnityEngine;

public class Reactions : MonoBehaviour
{
    public ElementDatabase elementDatabase;

    public static Reactions Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public Element GetReactionResultFor(GameObject hit)
    {
        var elements = hit.GetComponent<ElementStatus>();
        if (elements == null || elements.currentElements.Count < 2)
        {
            return null;
        }

        var e1 = elements.currentElements[0];
        var e2 = elements.currentElements[1];
        if (e1.elementTier != e2.elementTier)
        {
            return null;
        }

        return elementDatabase.GetComboResult(e1, e2);
    }

    public bool TryGetReactionResult(GameObject hit, out Element result)
    {
        result = GetReactionResultFor(hit);
        return result != null;
    }   

    public void TryApplyReaction(GameObject hit, ElementBehaviorContext context)
    {
        if (TryGetReactionResult(hit, out var reactionElement))
        {
            var elements = hit.GetComponent<ElementStatus>();
            var e1 = elements.currentElements[0];
            var e2 = elements.currentElements[1];
            elements.currentElements.RemoveAt(0);
            elements.currentElements.RemoveAt(0);
            elements.currentElements.Add(reactionElement);

            // Remove statuses from the original elements
            foreach (var behavior1 in e1.behaviors)
            {
                if (behavior1.StatusType != null && context.target.GetComponent(behavior1.StatusType) != null)
                {
                    var status = context.target.GetComponent(behavior1.StatusType);
                    Destroy(status);
                }
            }
            foreach (var behavior2 in e2.behaviors)
            {
                if (behavior2.StatusType != null && context.target.GetComponent(behavior2.StatusType) != null)
                {
                    var status = context.target.GetComponent(behavior2.StatusType);
                    Destroy(status);
                }
            }

            foreach (var behavior in reactionElement.behaviors)
            {
                behavior.Apply(context);
            }

            Debug.Log($"Reaction occurred! Created element: {reactionElement.elementName}");
        }
    }
}
