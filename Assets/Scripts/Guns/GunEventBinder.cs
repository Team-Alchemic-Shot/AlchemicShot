using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class GunEventBinder : MonoBehaviour
{
    [SerializeField]
    private Inventory inventory;

    private Gun subscribedGun;

    private void Awake()
    {
        if (inventory == null)
        {
            inventory = GetComponent<Inventory>();
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
    }
    

    private void UnsubscribeFromGun()
    {
        if (subscribedGun == null)
        {
            return;
        }

        subscribedGun.Fired -= AudioManager.PlayGunfire;
        subscribedGun.ReloadStarted -= AudioManager.PlayReload;
        subscribedGun.HitTarget -= Reactions.TryApplyReaction;
        subscribedGun.HitTarget -= ElementBehavior.ApplyBehaviors;
        subscribedGun = null;
    }
}
