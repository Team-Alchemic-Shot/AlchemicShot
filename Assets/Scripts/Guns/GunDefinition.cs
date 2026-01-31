using UnityEngine;

/// <summary>
/// Definition of a gun, including its stats, effects, and load/fire mechanism.
/// </summary>
[CreateAssetMenu(fileName = "New Gun", menuName = "Gun/Gun Definition")]
public class GunDefinition : ScriptableObject
{
    public string gunName = "New Gun";
    public LoadFireMechanism loadFireMechanism;
    public GunStats stats;
    public GunFX fx;
    public GameObject bulletPrefab;
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
[System.Serializable]
public struct GunStats
{
    public float damage;
    public float range;
    public float bulletLifeTime;
    public float bulletSpeed;
    public float fireRate;
    public int magazineSize;
    public float reloadTime;
}

/// <summary>
/// Struct to hold gun effects.
/// </summary>
[System.Serializable]
public struct GunFX
{
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public float shootSoundVolume;
    public float reloadSoundVolume;
}

