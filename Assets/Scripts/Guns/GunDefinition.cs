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
}

/// <summary>
/// Abstract base class for load and fire mechanisms of guns.
/// Implement specific loading and firing behaviors by extending this class.
/// </summary>
public abstract class LoadFireMechanism : ScriptableObject
{
    protected MagazineBlueprint magazineBlueprint;
    protected MagazineState magazineState;
    protected GameObject source;

    /// <summary>
    /// Initializes the load/fire mechanism with the given magazine blueprint and state references.
    /// </summary>
    public void Initialize(MagazineBlueprint bp, MagazineState state, GameObject source)
    {
        magazineBlueprint = bp;
        magazineState = state;
        this.source = source;
    }

    /// <summary>
    /// Loads ammunition from the given ammo stock into the magazine.
    /// </summary>
    public abstract void Load(int ammoStock);

    /// <summary>
    /// Fires the gun, returning the number of bullets fired.
    /// </summary>
    public abstract int Fire();
}

/// <summary>
/// Struct to hold gun statistics.
/// </summary>
[System.Serializable]
public struct GunStats
{
    public float damage;
    public float range;
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
}

