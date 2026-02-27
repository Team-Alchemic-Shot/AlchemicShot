using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : MonoBehaviour
{
    public GunDefinition gunDefinition;
    public MagazineBlueprint magazineBlueprint;

    public int AmmoStock { get; private set; } = 100;
    public LoadFireMechanism Mechanism { get; private set; }

    public event Action<GunDefinition, GameObject> Fired;
    public event Action<GunDefinition, GameObject> ReloadStarted;

    public event Action<ElementBehaviorContext> HitTarget;
    public event Action<BulletData, GunDefinition, GameObject> FiredBullet;
    public event Action<Ray, RaycastHit> HitSomething;
    public event Action<int, MagazineState> Reloaded;

    [Header("Debug")]
    [SerializeField]
    private bool drawMuzzleFlashGizmo = true;

    [SerializeField]
    private float muzzleGizmoRadius = 0.04f;

    private GameObject player;
    private InputAction fireAction;
    private InputAction reloadAction;
    private MagazineState magazineState;
    private bool isReloading;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player object not found in the scene. Please ensure there is a GameObject tagged 'Player'.");
        }

        // get controls
        fireAction = ControlUtil.FindProjectAction("Fire");
        reloadAction = ControlUtil.FindProjectAction("Reload");
        
        magazineBlueprint ??= new(gunDefinition.stats.magazineSize); // init blueprint if not set from editor
        magazineState = new(); // new empty magazine state

        // scriptable objects are stored on the disk, so we need to instantiate them to get a unique instance
        Mechanism = Instantiate(gunDefinition.loadFireMechanism);
        Mechanism.Initialize( // set references
            magazineBlueprint, 
            magazineState,
            gunDefinition,
            player,
            gunDefinition.bulletPrefab);

        // transfer to facade
        Mechanism.HitTarget += OnMechanismHitTarget;
        Mechanism.FiredBullet += OnMechanismFiredBullet;
        Mechanism.HitSomething += OnMechanismHitSomething;
        Mechanism.Reloaded += OnMechanismReloaded;

        Mechanism.Load(AmmoStock); // load initial magazine
    }

    private void OnDestroy()
    {
        if (Mechanism == null)
        {
            return;
        }

        Mechanism.HitTarget -= OnMechanismHitTarget;
        Mechanism.FiredBullet -= OnMechanismFiredBullet;
        Mechanism.HitSomething -= OnMechanismHitSomething;
        Mechanism.Reloaded -= OnMechanismReloaded;
    }

    private void OnDisable()
    {
        if (isReloading)
        {
            StopAllCoroutines();
            isReloading = false;
        }
    }

    private void OnMechanismHitTarget(ElementBehaviorContext context) => HitTarget?.Invoke(context);
    private void OnMechanismFiredBullet(BulletData bullet, GunDefinition definition, GameObject source) => FiredBullet?.Invoke(bullet, definition, source);
    private void OnMechanismHitSomething(Ray ray, RaycastHit hit) => HitSomething?.Invoke(ray, hit);
    private void OnMechanismReloaded(int ammoLoaded, MagazineState state) => Reloaded?.Invoke(ammoLoaded, state);

    private void Update()
    {
        if (gunDefinition.fireMode == FireMode.FullAuto)
        {
            if (fireAction.IsPressed())
            {
                Fire();
            }
        }
        else if (fireAction.triggered)
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

        if (magazineState.Count > 0 && AmmoStock != 0)
        {
            var bulletsFired = Mechanism.Fire(AmmoStock);
            if (bulletsFired > 0)
            {
                Fired?.Invoke(gunDefinition, player);
            }
            if (magazineState.Count == 0)
            {
                StartReload();
            }
        }
        else if (AmmoStock != 0)
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

        Mechanism.Load(AmmoStock);
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
                magazineState.Push(magazineBlueprint.bullets[i].Clone());
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!drawMuzzleFlashGizmo || gunDefinition == null)
        {
            return;
        }

        GameObject playerRef = player;
        if (playerRef == null)
        {
            playerRef = GameObject.FindGameObjectWithTag("Player");
        }

        if (!MuzzleFlashAnimator.TryGetMuzzleFlashPose(gunDefinition, playerRef, out Vector3 muzzlePos, out _))
        {
            return;
        }

        Transform anchor = transform;
        if (playerRef != null)
        {
            var gunLook = playerRef.GetComponentInChildren<GunLook>();
            if (gunLook != null)
            {
                anchor = gunLook.transform;
            }
            else
            {
                anchor = playerRef.transform;
            }
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(anchor.position, muzzlePos);
        Gizmos.DrawSphere(muzzlePos, muzzleGizmoRadius);
    }
}