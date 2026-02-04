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

    private bool TryGetReactionResult(GameObject hit, out ElementCombo elementCombo)
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

    private void TryApplyReaction(ElementBehaviorContext context)
    {
        if (TryGetReactionResult(context.target, out var elementCombo))
        {
            var elements = context.target.GetComponent<ElementStatus>();
            var e1 = elements.currentElements[0];
            var e2 = elements.currentElements[1];
            elements.currentElements.RemoveAt(0);
            elements.currentElements.RemoveAt(0);
            elements.currentElements.Add(elementCombo.resultElement);

            var result = elementCombo.resultElement;

            // Remove statuses from the original elements
            if (elementCombo.removeOldStatusesOnReaction)
            {
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
            }

            foreach (var behavior in result.behaviors)
            {
                behavior.Apply(context);
            }

            Debug.Log($"Reaction occurred! Created element: {result.elementName}");
        }
    }

    public static void DoReaction(ElementBehaviorContext context)
    {
        if (!context.target.TryGetComponent<ElementStatus>(out var elementStatus))
        {
            elementStatus = context.target.AddComponent<ElementStatus>();
        }
        elementStatus.currentElements.Add(context.sourceBullet.element); // track element
        Instance.TryApplyReaction(context);
    }
}
