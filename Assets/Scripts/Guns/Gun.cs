using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : MonoBehaviour
{
    public GunDefinition gunDefinition;
    public ElementDatabase elementDatabase;
    public GameObject player;
    public MagazineBlueprint magazineBlueprint;

    public event Action<GunDefinition, GameObject> Fired;
    public event Action<GunDefinition, GameObject> ReloadStarted;

    public event Action<ElementBehaviorContext> HitTarget;
    public event Action<BulletData> FiredBullet;
    public event Action<Ray, RaycastHit> HitSomething;
    public event Action<(int, MagazineState)> Reloaded;

    private InputAction fireAction;
    private InputAction reloadAction;
    private MagazineState magazineState;
    private LoadFireMechanism mechanism;
    private int ammoStock = 999999999; // infinite ammo for now?
    private bool isReloading;

    private void Awake()
    {
        // get controls
        fireAction = ControlUtil.FindProjectAction("Fire");
        reloadAction = ControlUtil.FindProjectAction("Reload");
        
        magazineBlueprint ??= new(gunDefinition.stats.magazineSize); // init blueprint if not set from editor
        magazineState = new(); // new empty magazine state

        // scriptable objects are stored on the disk, so we need to instantiate them to get a unique instance
        mechanism = Instantiate(gunDefinition.loadFireMechanism);
        mechanism.Initialize( // set references
            magazineBlueprint, 
            magazineState,
            gunDefinition.stats,
            gunDefinition.fx, 
            player,
            gunDefinition.bulletPrefab);

        mechanism.HitTarget += OnMechanismHitTarget;
        mechanism.FiredBullet += OnMechanismFiredBullet;
        mechanism.HitSomething += OnMechanismHitSomething;
        mechanism.Reloaded += OnMechanismReloaded;

        mechanism.Load(ammoStock); // load initial magazine
    }

    private void OnDestroy()
    {
        if (mechanism == null)
        {
            return;
        }

        mechanism.HitTarget -= OnMechanismHitTarget;
        mechanism.FiredBullet -= OnMechanismFiredBullet;
        mechanism.HitSomething -= OnMechanismHitSomething;
        mechanism.Reloaded -= OnMechanismReloaded;
    }

    private void OnMechanismHitTarget(ElementBehaviorContext context) => HitTarget?.Invoke(context);
    private void OnMechanismFiredBullet(BulletData bullet) => FiredBullet?.Invoke(bullet);
    private void OnMechanismHitSomething(Ray ray, RaycastHit hit) => HitSomething?.Invoke(ray, hit);
    private void OnMechanismReloaded((int, MagazineState) payload) => Reloaded?.Invoke(payload);

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
            var bulletsFired = mechanism.Fire(ammoStock);
            if (bulletsFired > 0)
            {
                Fired?.Invoke(gunDefinition, player);
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

        ReloadStarted?.Invoke(gunDefinition, player);
        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        float reloadTime = Mathf.Max(0f, gunDefinition.stats.reloadTime);
        if (reloadTime > 0f)
        {
            yield return new WaitForSeconds(reloadTime);
        }

        mechanism.Load(ammoStock);
        isReloading = false;
    }
}