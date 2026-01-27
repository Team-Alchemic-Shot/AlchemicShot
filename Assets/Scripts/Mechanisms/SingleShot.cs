using UnityEngine;

[CreateAssetMenu(fileName = "SingleShot", menuName = "Gun/LoadFireMechanism/SingleShot")]
public class SingleShot : LoadFireMechanism
{
    [SerializeField]
    private float range = 50f;
    [SerializeField]
    private LayerMask zombieMask;

    public override int Fire()
    {
        if (magazineState.Count == 0)
        {
            Debug.LogWarning("No bullets to fire!");
            return 0;
        }
        Debug.Log("SingleShot Fire");

        var bullet = magazineState.Pop();

        // raycast (only hits Zombie layer)
        Camera cam = Camera.main;
        if (cam != null)
        {
            Ray ray = new(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, range, zombieMask))
            {
                Debug.Log($"Hit zombie: {hit.collider.name}");
                if (hit.collider.TryGetComponent<Health>(out var health))
                {
                    health.ApplyDamage(new DamageInfo
                    {
                        amount = bullet.baseDamage,
                        source = source,
                        position = hit.point
                    });
                }
            }
        }
       

        return 1;
    }

    public override void Load(int ammoStock)
    {
        Debug.Log("SingleShot Load");
        if (ammoStock == 0)
        {
            Debug.LogWarning("No ammo stock to load from!");
            return;
        }

        if (magazineBlueprint == null || magazineBlueprint.bullets == null)
        {
            Debug.LogWarning("No magazine blueprint configured.");
            return;
        }

        if (magazineState == null)
        {
            Debug.LogWarning("No magazine state configured.");
            return;
        }

        magazineState.Clear();

        int bulletsToLoad = Mathf.Min(ammoStock, magazineBlueprint.bullets.Length);
        
        // Stack pops last-in-first-out, so push in reverse to fire in blueprint order.
        for (int i = bulletsToLoad - 1; i >= 0; i--)
        {
            magazineState.Push(magazineBlueprint.bullets[i]);
        }
    }
}