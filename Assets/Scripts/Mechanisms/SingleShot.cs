using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "SingleShot", menuName = "Gun/LoadFireMechanism/SingleShot")]
public class SingleShot : LoadFireMechanism
{
    private static WaitForSeconds _waitForSeconds0_1 = new WaitForSeconds(0.1f);
    [SerializeField]
    private LayerMask zombieMask;

    public override int Fire(int ammoStock)
    {
        if (magazineState.Count == 0)
        {
            Debug.LogWarning("No bullets to fire!");
            return 0;
        }

        var bullet = magazineState.Pop();
        while (bullet.isEmpty)
        {
            if (magazineState.Count == 0)
            {
                return 0;
            }

            bullet = magazineState.Pop();
        }

        var bulletObj = Instantiate(bulletPrefab, Camera.main.transform.position, source.transform.rotation);
        bulletObj.GetComponent<BulletScript>()
            .Initialize(gunStats.bulletLifeTime, Camera.main.transform.forward, gunStats.bulletSpeed);

        // raycast (only hits Zombie layer)
        Camera cam = Camera.main;
        if (cam != null)
        {
            Ray ray = new(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, gunStats.range, zombieMask))
            {
                if (hit.collider.TryGetComponent<Health>(out var health))
                {
                    float healthBefore = health.CurrentHealth;
                    health.ApplyDamage(new DamageInfo
                    {
                        amount = bullet.baseDamage + gunStats.damage,
                        source = source,
                        position = hit.point
                    });
                    foreach (var behavior in bullet.element.behaviors)
                    {
                        var context = new ElementBehaviorContext
                        {
                            instigator = source,
                            target = hit.collider.gameObject,
                            position = hit.point
                        };
                        behavior.Apply(context);
                    }
                    Debug.Log($"Entity damage taken: {healthBefore} -> {health.CurrentHealth}");
                }
                Debug.DrawLine(ray.origin, hit.point, Color.red, 5f);
                Destroy(bulletObj);
            }
        }

        return 1;
    }

    public override void Load(int ammoStock)
    {
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
        Reload(ammoStock);
    }

    private void Reload(int ammoStock)
    {
        int bulletsToLoad = Mathf.Min(ammoStock, magazineBlueprint.bullets.Length);
        
        // Stack pops last-in-first-out, so push in reverse to fire in blueprint order.
        for (int i = bulletsToLoad - 1; i >= 0; i--)
        { 
            magazineState.Push(magazineBlueprint.bullets[i]);
        }
    }
}