using UnityEngine;

public static class MuzzleFlashAnimator
{
    /// <summary>
    /// Number of particles emitted in the muzzle-flash burst.
    /// </summary>
    private const short BurstCount = 8;

    public static void PlayMuzzleFlash(GunDefinition definition, GameObject player)
    {
        if (definition == null || definition.fx.muzzleFlash.prefab == null)
        {
            return;
        }

        var fx = definition.fx.muzzleFlash;
        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        // Compute world-space spawn point from camera-local offset
        Transform camT = cam.transform;
        Vector3 worldPos = camT.position + camT.TransformDirection(fx.muzzleOffset);
        Quaternion worldRot = camT.rotation;

        // Spawn detached from the camera so it isn't culled by the near plane
        var flash = Object.Instantiate(fx.prefab, worldPos, worldRot);

        // Ensure the particle system uses burst emission (not rate-over-distance)
        // so the flash fires even when the player is standing still.
        var ps = flash.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            var emission = ps.emission;
            emission.rateOverDistance = 0f;
            emission.rateOverTime = 0f;
            emission.burstCount = 1;
            emission.SetBurst(0, new ParticleSystem.Burst(0f, BurstCount));

            // Use local simulation so particles stay at the flash origin
            var main = ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            // Restart so the new settings take effect immediately
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play();
        }

        Object.Destroy(flash, fx.duration);
    }
}