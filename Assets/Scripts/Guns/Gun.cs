using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : MonoBehaviour
{
    public GunDefinition gunDefinition;
    public MagazineBlueprint magazineBlueprint;
    public ElementDatabase elementDatabase;
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
        magazineBlueprint.bullets[0] = new BulletData
        {
            element = elementDatabase.GetElementByName("Air"),
            baseDamage = gunDefinition.stats.damage,
            isEmpty = false
        };
        magazineBlueprint.bullets[1] = new BulletData
        {
            element = elementDatabase.GetElementByName("Fire"),
            baseDamage = gunDefinition.stats.damage,
            isEmpty = false
        };
        magazineBlueprint.bullets[2] = new BulletData
        {
            element = elementDatabase.GetElementByName("Water"),
            baseDamage = gunDefinition.stats.damage,
            isEmpty = false
        };
        magazineBlueprint.bullets[3] = new BulletData
        {
            element = elementDatabase.GetElementByName("Earth"),
            baseDamage = gunDefinition.stats.damage,
            isEmpty = false
        };        
        magazineBlueprint.bullets[4] = new BulletData
        {
            element = elementDatabase.GetElementByName("Air"),
            baseDamage = gunDefinition.stats.damage,
            isEmpty = false
        };
        magazineBlueprint.bullets[5] = new BulletData
        {
            element = elementDatabase.GetElementByName("Fire"),
            baseDamage = gunDefinition.stats.damage,
            isEmpty = false
        };        

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

    public MagazineState GetMagazine()
    {
        return magazineState;
    }

    public MagazineBlueprint GetMagazineBlueprint()
    {
        return magazineBlueprint;
    }

    public void UpdateMagazineFromBlueprint()
    {
        // Clear the current magazine state and reload from blueprint
        magazineState.Clear();
        // Load bullets in reverse order so they stack correctly (last bullet is fired first)
        for (int i = magazineBlueprint.bullets.Length - 1; i >= 0; i--)
        {
            if (magazineBlueprint.bullets[i] != null)
            {
                magazineState.Push(magazineBlueprint.bullets[i]);
            }
        }
    }
}