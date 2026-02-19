using System;
using UnityEngine;

/// <summary>
/// Definition of a gun, including its stats, effects, and load/fire mechanism.
/// </summary>
[CreateAssetMenu(fileName = "New Gun", menuName = "Gun/Gun Definition")]
public class GunDefinition : ScriptableObject
{
    public string gunName = "New Gun";
    public LoadFireMechanism loadFireMechanism;
    public FireMode fireMode = FireMode.SemiAuto;
    public GunStats stats;
    public GunFX fx;
    public GameObject bulletPrefab;
}

public enum FireMode
{
    SemiAuto,
    FullAuto
}

/// <summary>
/// Abstract base class for load and fire mechanisms of guns.
/// Implement specific loading and firing behaviors by extending this class.
/// </summary>
public abstract class LoadFireMechanism : ScriptableObject
{
    protected MagazineBlueprint magazineBlueprint;
    protected MagazineState magazineState;
    protected GunStats gunStats;
    protected GunFX gunFX;
    protected GameObject source;
    protected GameObject bulletPrefab;

    public event Action<ElementBehaviorContext> HitTarget;
    public event Action<BulletData> FiredBullet;
    public event Action<Ray, RaycastHit> HitSomething;
    public event Action<int, MagazineState> Reloaded;

    /// <summary>
    /// Initializes the load/fire mechanism with the given magazine blueprint and state references.
    /// </summary>
    public void Initialize(
        MagazineBlueprint bp, 
        MagazineState state, 
        GunStats stats, 
        GunFX fx, 
        GameObject src,
        GameObject bullet)
    {
        magazineBlueprint = bp;
        magazineState = state;
        gunStats = stats;
        gunFX = fx;
        source = src;
        bulletPrefab = bullet;
    }

    protected void NotifyFiredBullet(BulletData bullet)
    {
        FiredBullet?.Invoke(bullet);
    }
    
    protected void NotifyHitTarget(ElementBehaviorContext context)
    {
        HitTarget?.Invoke(context);
    }

    protected void NotifyHitSomething(Ray ray, RaycastHit hit)
    {
        HitSomething?.Invoke(ray, hit);
    }

    protected void NotifyReloaded(int ammoLoaded, MagazineState state)
    {
        Reloaded?.Invoke(ammoLoaded, state);
    }

    /// <summary>
    /// Get spread-adjusted shot direction based on the gun's stats.
    /// </summary>
    /// <param name="camTransform"></param>
    /// <returns></returns>
    protected Vector3 GetShotDirection(Transform camTransform)
    {
        if (gunStats.spread <= 0f)
        {
            return camTransform.forward;
        }

        float half = gunStats.spread * 0.5f;
        float yaw = UnityEngine.Random.Range(-half, half);
        float pitch = UnityEngine.Random.Range(-half, half);
        return Quaternion.Euler(pitch, yaw, 0f) * camTransform.forward;
    }
    
    /// <summary>
    /// Loads ammunition from the given ammo stock into the magazine.
    /// </summary>
    public abstract void Load(int ammoStock);

    /// <summary>
    /// Fires the gun, returning the number of bullets fired.
    /// </summary>
    public abstract int Fire(int ammoStock);
}

/// <summary>
/// Struct to hold gun statistics.
/// </summary>
[Serializable]
public struct GunStats
{
    public float damage;
    public float range;
    public float bulletLifeTime;
    public float bulletSpeed;
    public float fireRate;
    public int magazineSize;
    public float reloadTime;
    public float spread;
}

/// <summary>
/// Struct to hold gun effects.
/// </summary>
[Serializable]
public struct GunFX
{
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public float shootSoundVolume;
    public float reloadSoundVolume;
}

