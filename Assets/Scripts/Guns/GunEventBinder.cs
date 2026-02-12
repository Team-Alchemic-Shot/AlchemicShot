using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class GunEventBinder : MonoBehaviour
{
    [SerializeField]
    private Inventory inventory;

    [SerializeField]
    private GameObject magazineUIObject;

    private MagazineUI magazineUI;

    private Gun subscribedGun;

    private void Awake()
    {
        if (inventory == null)
        {
            inventory = GetComponent<Inventory>();
        }
        if (magazineUIObject != null)
        {
            magazineUI = magazineUIObject.GetComponent<MagazineUI>();
        }
    }

    private void OnEnable()
    {
        if (inventory != null)
        {
            inventory.OnGunChanged += HandleGunChanged;
        }
    }

    private void Start()
    {
        if (inventory != null)
        {
            HandleGunChanged(inventory.CurrentGun);
        }
    }

    private void OnDisable()
    {
        if (inventory != null)
        {
            inventory.OnGunChanged -= HandleGunChanged;
        }

        UnsubscribeFromGun();
    }

    private void HandleGunChanged(Gun gun)
    {
        UnsubscribeFromGun();
        subscribedGun = gun;

        if (subscribedGun == null)
        {
            return;
        }

        subscribedGun.Fired += AudioManager.PlayGunfire;
        subscribedGun.ReloadStarted += AudioManager.PlayReload;
        subscribedGun.HitTarget += ElementBehavior.ApplyBehaviors;
        subscribedGun.HitTarget += Reactions.TryApplyReaction; // do reactions after applying behaviors
        subscribedGun.HitTarget += ElementImpactFx.TryPlayOnHit; // do VFX/SFX after reactions so result element wins
        subscribedGun.FiredBullet += magazineUI.OnFired;
        subscribedGun.Reloaded += magazineUI.OnReloaded;
    }
    

    private void UnsubscribeFromGun()
    {
        if (subscribedGun == null)
        {
            return;
        }

        subscribedGun.Fired -= AudioManager.PlayGunfire;
        subscribedGun.ReloadStarted -= AudioManager.PlayReload;
        subscribedGun.HitTarget -= ElementImpactFx.TryPlayOnHit;
        subscribedGun.HitTarget -= Reactions.TryApplyReaction;
        subscribedGun.HitTarget -= ElementBehavior.ApplyBehaviors;
        subscribedGun.FiredBullet -= magazineUI.OnFired;
        subscribedGun.Reloaded -= magazineUI.OnReloaded;
        subscribedGun = null;
    }
}
