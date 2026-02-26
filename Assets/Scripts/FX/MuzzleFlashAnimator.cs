using UnityEngine;

public static class MuzzleFlashAnimator
{
    public static void PlayMuzzleFlash(GunDefinition definition, GameObject _)
    {
        if (definition == null || definition.muzzleFlashFX.prefab == null)
        {
            return;
        }

        var fx = definition.muzzleFlashFX;

        var flash = Object.Instantiate(fx.prefab);
        flash.transform.localPosition = fx.muzzleOffset;
        Object.Destroy(flash, fx.duration);
    }
}