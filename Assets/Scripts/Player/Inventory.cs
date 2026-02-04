using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    
    public Gun currentGun;
    public List<Gun> guns = new();

    public event Action<Gun> OnGunChanged;

    private Gun subscribedGun;

    void Awake()
    {
        OnGunChanged += gun => SubscribeToGunEvents(gun);
    }

    private void Start()
    {
        OnGunChanged?.Invoke(currentGun);
    }

    void Update()
    {
        // switch guns check and invoke event
    }

    private void SubscribeToGunEvents(Gun gun)
    {
        if (subscribedGun != null)
        {
            subscribedGun.Fired -= GunSounds.PlayGunfire;
            subscribedGun.ReloadStarted -= GunSounds.PlayReload;
            subscribedGun.HitTarget -= Reactions.DoReaction;
            subscribedGun.HitTarget -= ElementBehavior.ApplyBehaviors;
        }

        subscribedGun = gun;
        if (subscribedGun == null)
        {
            return;
        }

        subscribedGun.Fired += GunSounds.PlayGunfire;
        subscribedGun.ReloadStarted += GunSounds.PlayReload;
        subscribedGun.HitTarget += Reactions.DoReaction;
        subscribedGun.HitTarget += ElementBehavior.ApplyBehaviors;
    }

    private void OnDestroy()
    {
        SubscribeToGunEvents(null);
    }
}