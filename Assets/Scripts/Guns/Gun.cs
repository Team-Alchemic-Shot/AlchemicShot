using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : MonoBehaviour
{
    public GunDefinition gunDefinition;
    public MagazineBlueprint magazineBlueprint;
    public GameObject player;

    private InputAction fireAction;
    private InputAction reloadAction;
    private MagazineState magazineState = new();
    private int ammoStock = 999999999; // infinite ammo for now?
    private bool isReloading;

    private void Awake()
    {
        fireAction = ControlUtil.FindProjectAction("Fire");
        reloadAction = ControlUtil.FindProjectAction("Reload");
        
        magazineBlueprint = new()
        {
            bullets = new BulletData[gunDefinition.stats.magazineSize]
        };
        test_LoadBP();
        gunDefinition.loadFireMechanism.Initialize(
            magazineBlueprint, 
            magazineState,
            gunDefinition.stats,
            gunDefinition.fx, 
            player);
        gunDefinition.loadFireMechanism.Load(ammoStock);
    }

    private void test_LoadBP()
    {
        for (int i = 0; i < magazineBlueprint.bullets.Length; i++)
        {
            magazineBlueprint.bullets[i] = new BulletData
            {
                baseDamage = 100,
                isEmpty = false
            };
        }
    }

    private void Update()
    {
        if (fireAction.triggered)
        {
            Fire();
        }

        if (reloadAction.triggered)
        {
            StartReload();
        }
    }

    private void Fire()
    {
        if (isReloading)
        {
            return;
        }

        if (magazineState.Count > 0 && ammoStock != 0)
        {
            // ammoStock -= gunDefinition.loadFireMechanism.Fire(); infinite ammo for now
            gunDefinition.loadFireMechanism.Fire(ammoStock);
            if (gunDefinition.fx.shootSound != null)
            {
                AudioSource.PlayClipAtPoint(
                    gunDefinition.fx.shootSound,
                    player.transform.position,
                    gunDefinition.fx.shootSoundVolume);
            }
            if (magazineState.Count == 0)
            {
                StartReload();
            }
        }
        else if (ammoStock != 0)
        {
            StartReload();
        }
    }

    private void StartReload()
    {
        if (isReloading)
        {
            return;
        }

        isReloading = true;
        if (gunDefinition.fx.reloadSound != null)
        {
            AudioSource.PlayClipAtPoint(
                gunDefinition.fx.reloadSound,
                player.transform.position,
                gunDefinition.fx.reloadSoundVolume);
        }
        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        float reloadTime = Mathf.Max(0f, gunDefinition.stats.reloadTime);
        if (reloadTime > 0f)
        {
            yield return new WaitForSeconds(reloadTime);
        }

        gunDefinition.loadFireMechanism.Load(ammoStock);
        isReloading = false;
    }
}