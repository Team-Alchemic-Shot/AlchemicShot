using UnityEngine;

[CreateAssetMenu(fileName = "SingleShot", menuName = "Gun/LoadFireMechanism/SingleShot")]
public class SingleShot : LoadFireMechanism
{
    [SerializeField]
    private LayerMask zombieMask;

    public override int Fire(int ammoStock)
    {
        if (magazineState.Count == 0)
        {
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
        var bs = bulletObj.GetComponent<BulletScript>();
        bs.Initialize(gunStats.bulletLifeTime, Camera.main.transform.forward, gunStats.bulletSpeed);

        NotifyFiredBullet(bullet);

        // raycast (only hits Zombie and Default layer)
        Camera cam = Camera.main;
        Ray ray = new(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, gunStats.range, zombieMask))
        {
            if (hit.collider.TryGetComponent<Health>(out var health))
            {
                // apply bullet damage
                health.ApplyDamage(new DamageInfo
                {
                    amount = bullet.baseDamage + gunStats.damage,
                    source = source,
                    position = hit.point
                });
                
                // create a context to pass to element behaviors and reactions
                var context = new ElementBehaviorContext
                {
                    instigator = source,
                    target = hit.collider.gameObject,
                    position = hit.point,
                    sourceBullet = bullet
                };

                NotifyHitTarget(context);
            }
            NotifyHitSomething(ray, hit);
            bs.SetLifetime(0.5f); // rough hack to make bullet disappear quickly after hit
        }

        return 1;
    }

    public override void Load(int ammoStock)
    {
        if (ammoStock == 0)
        {
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
                var blueprintBullet = magazineBlueprint.bullets[i];
                if (blueprintBullet == null)
                {
                    magazineState.Push(new BulletData { isEmpty = true });
                }
                else
                {
                    magazineState.Push(blueprintBullet.Clone()); // prevent reference issues by cloning bullets from blueprint
                }
        }
        NotifyReloaded(bulletsToLoad, magazineState);
    }
}