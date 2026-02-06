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

        // stick mesh color change here actually

        if (element.sfxClip != null)
        {
            AudioSource.PlayClipAtPoint(
                element.sfxClip,
                context.position,
                element.sfxVolume);
        }
    }
}
