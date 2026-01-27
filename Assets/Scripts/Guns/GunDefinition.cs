using UnityEngine;

[CreateAssetMenu(fileName = "New Gun", menuName = "Gun/Gun Definition")]
public class GunDefinition : ScriptableObject
{
    public string gunName = "New Gun";
    public LoadFireMechanism loadFireMechanism;
    public GunStats stats;
    public GunFX fx;
}

public abstract class LoadFireMechanism : ScriptableObject
{
    protected MagazineBlueprint magazineBlueprint;
    protected MagazineState magazineState;

    public void Initialize(MagazineBlueprint bp, MagazineState state)
    {
        magazineBlueprint = bp;
        magazineState = state;
    }

    public abstract void Load(int ammoStock);
    public abstract int Fire();
}

[System.Serializable]
public struct GunStats
{
    public float damage;
    public float range;
    public float fireRate;
    public int magazineSize;
    public float reloadTime;
}

[System.Serializable]
public struct GunFX
{
    public GameObject bulletPrefab;
    public AudioClip shootSound;
    public AudioClip reloadSound;
}

