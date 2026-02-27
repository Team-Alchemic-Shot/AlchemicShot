using UnityEngine;

public static class MuzzleFlashAnimator
{
    /// <summary>
    /// Number of particles emitted in the muzzle-flash burst.
    /// </summary>
    private const short BurstCount = 8;

    private static void ApplyMaterialColor(Material material, Color color)
    {
        if (material == null)
        {
            return;
        }

        material.color = color;
    }

    public static bool TryGetMuzzleFlashPose(GunDefinition definition, GameObject player, out Vector3 worldPos, out Quaternion worldRot)
    {
        worldPos = Vector3.zero;
        worldRot = Quaternion.identity;

        if (definition == null || definition.fx.muzzleFlash.prefab == null)
        {
            return false;
        }

        Transform anchor = null;
        if (player != null)
        {
            var gunLook = player.GetComponentInChildren<GunLook>();
            if (gunLook != null)
            {
                anchor = gunLook.transform;
            }
        }

        if (anchor == null)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                anchor = cam.transform;
            }
        }

        if (anchor == null)
        {
            return false;
        }

        var fx = definition.fx.muzzleFlash;
        worldPos = anchor.TransformPoint(fx.muzzleOffset);
        worldRot = anchor.rotation;
        return true;
    }

    public static void PlayMuzzleFlash(BulletData data, GunDefinition definition, GameObject player)
    {
        if (!TryGetMuzzleFlashPose(definition, player, out Vector3 worldPos, out Quaternion worldRot))
        {
            return;
        }

        Color color;
        if (data.element == null)
        {
            color = Color.white;
            Debug.LogWarning("BulletData has null element. Defaulting muzzle flash color to white.", player);
        }
        else
        {
            color = data.element.elementColor;
        }

        color.a = 1f;

        var fx = definition.fx.muzzleFlash;

        // Spawn detached from the camera so it isn't culled by the near plane
        var flash = Object.Instantiate(fx.prefab, worldPos, worldRot);

        // Ensure particle systems use burst emission (not rate-over-distance),
        // then tint both particle start color and renderer material.
        var particleSystems = flash.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particleSystems.Length; i++)
        {
            var ps = particleSystems[i];
            var emission = ps.emission;
            emission.rateOverDistance = 0f;
            emission.rateOverTime = 0f;
            emission.burstCount = 1;
            emission.SetBurst(0, new ParticleSystem.Burst(0f, BurstCount));

            // Use local simulation so particles stay at the flash origin
            var main = ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            // Tint particles with the element color
            main.startColor = new ParticleSystem.MinMaxGradient(color);

            if (ps.TryGetComponent<ParticleSystemRenderer>(out var particleRenderer))
            {
                ApplyMaterialColor(particleRenderer.material, color);
            }

            // Restart so the new settings take effect immediately
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play();
        }

        // Tint any lights on the flash prefab
        foreach (var light in flash.GetComponentsInChildren<Light>(true))
        {
            light.color = color;
        }

        Object.Destroy(flash, fx.duration);
    }
}