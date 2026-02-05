using UnityEngine;

public abstract class ElementTag : MonoBehaviour
{
    public abstract void Apply(
            float duration, 
            float interval, 
            float intensity, 
            bool refreshDuration, 
            bool stackIntensity,
            GameObject instigator, 
            bool logTicks);

    protected virtual void OnDestroy()
    {
        if (TryGetComponent<ElementStatus>(out var elementStatus))
        {
            elementStatus.RemoveElementFromTag(GetType());
        }
    }
}