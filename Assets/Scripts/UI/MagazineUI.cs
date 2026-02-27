using UnityEngine;

public abstract class MagazineUI : MonoBehaviour
{
    public abstract void OnFired(BulletData data, GunDefinition definition, GameObject source);
    public abstract void OnReloaded(int ammo, MagazineState magazineState);
}