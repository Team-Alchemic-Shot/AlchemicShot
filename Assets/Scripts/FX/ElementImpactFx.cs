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
        if (context.targets == null || context.targets.Count == 0 || context.sourceBullet.element == null)
        {
            return;
        }

        foreach (var target in context.targets)
        {
            // reactions has updated the bullet's element to the reaction result, if one occurred
            var element = context.sourceBullet.element;

            if (element.vfxPrefab != null)
            {
                Object.Instantiate(
                    element.vfxPrefab,
                    context.position,
                    Quaternion.identity,
                    target.transform);
            }

            if (element.impactSound != null && element.impactSound.clip != null)
            {
                SoundManager.PlaySound(element.impactSound, target);
            }
        }
    }
}
