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

    private ElementCombo GetReactionResultFor(GameObject hit)
    {
        var status = hit.GetComponent<ElementStatus>();
        if (status == null || status.currentElements.Count < 2)
        {
            return null;
        }

        var e1 = status.currentElements[^2];
        var e2 = status.currentElements[^1];
        if (e1.elementTier != e2.elementTier)
        {
            return null;
        }

        return elementDatabase.GetComboResult(e1, e2);
    }

    public bool TryGetReactionResult(GameObject hit, out ElementCombo elementCombo)
    {
        var combo = GetReactionResultFor(hit);
        if (combo != null)
        {
            elementCombo = combo;
            return true;
        }
        elementCombo = null;
        return false;
    }

    public static void TryApplyReaction(ElementBehaviorContext context)
    {
        if (Instance.TryGetReactionResult(context.target, out var elementCombo))
        {
            var result = elementCombo.resultElement;
            var status = context.target.GetComponent<ElementStatus>(); // will always have a status
            var e1 = elementCombo.inputElements.elementA;
            var e2 = elementCombo.inputElements.elementB;

            status.AddElement(result); // track result

            // Remove statuses and revert effects from the original elements
            if (elementCombo.removeE1OldStatusesOnReaction)
            {
                foreach (var behavior1 in e1.behaviors)
                {
                    behavior1.Remove(context);
                }
                status.RemoveElement(e1);
            }
            if (elementCombo.removeE2OldStatusesOnReaction)
            {
                foreach (var behavior2 in e2.behaviors)
                {
                    behavior2.Remove(context);
                }
                status.RemoveElement(e2);
            } 

            foreach (var behavior in result.behaviors)
            {
                behavior.Apply(context);
            }

            Debug.Log($"Reaction occurred! Created element: {result.elementName}");
        }
    }
}
