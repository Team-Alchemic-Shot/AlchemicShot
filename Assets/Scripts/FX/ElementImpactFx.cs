using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// Plays element VFX/SFX for a hit after behaviors + reactions have resolved.
/// This is intentionally separate from ElementBehavior so presentation can be ordered
/// independently from gameplay/status application.
/// </summary>
public static class ElementImpactFx
{
    public static void TryPlayOnHit(ElementBehaviorContext context)
    {
        if (context.target == null || context.sourceBullet.element == null)
        {
            return;
        }

        // reactions has updated the bullet's element to the reaction result, if one occurred
        var element = context.sourceBullet.element;

        if (element.vfxPrefab != null)
        {
            Object.Instantiate(
                element.vfxPrefab,
                context.position,
                Quaternion.identity,
                context.target.transform);
        }

        // Apply mesh color shift based on element
        ApplyMeshColorShift(context.target, element);

        if (element.sfxClip != null)
        {
            AudioSource.PlayClipAtPoint(
                element.sfxClip,
                context.position,
                element.sfxVolume);
        }
    }

    private static void ApplyMeshColorShift(GameObject target, Element element)
    {
        if (target == null)
        {
            return;
        }

        if (!target.TryGetComponent<MeshRenderer>(out var meshRenderer))
        {
            meshRenderer = target.GetComponentInChildren<MeshRenderer>();
        }

        if (meshRenderer == null)
        {
            return;
        }

        // Store original color and apply new color
        Color originalColor = meshRenderer.material.color;
        meshRenderer.material.color = element.elementColor;

        // Calculate duration from behaviors
        float duration = 3f; // default
        if (element.behaviors != null && element.behaviors.Count > 0)
        {
            duration = element.behaviors.Max(b => b.duration);
        }

        // Schedule revert coroutine
        if (target.TryGetComponent<MonoBehaviour>(out var monoBehaviour))
        {
            monoBehaviour.StartCoroutine(RevertMeshColorAfterDelay(meshRenderer, originalColor, duration));
        }
    }

    private static IEnumerator RevertMeshColorAfterDelay(MeshRenderer renderer, Color originalColor, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (renderer != null)
        {
            renderer.material.color = originalColor;
        }
    }
}
