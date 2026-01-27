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
            player);
        gunDefinition.loadFireMechanism.Load(ammoStock);
    }

    private void test_LoadBP()
    {

        magazineBlueprint.bullets[0] = new BulletData();   
    }

    private void Update()
    {
        if (fireAction.triggered)
        {
            Fire();
        }

        if (reloadAction.triggered)
        {
            gunDefinition.loadFireMechanism.Load(ammoStock);
        }
    }

    private void Fire()
    {
        if (magazineState.Count > 0 && ammoStock != 0)
        {
            // ammoStock -= gunDefinition.loadFireMechanism.Fire(); infinite ammo for now
            gunDefinition.loadFireMechanism.Fire();
            if (magazineState.Count == 0)
            {
                gunDefinition.loadFireMechanism.Load(ammoStock);
            }
        }
    }
}