using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NBarrel", menuName = "Gun/LoadFireMechanism/NBarrel")]
public class NBarrel : LoadFireMechanism
{
    [SerializeField]
    private LayerMask zombieMask;

    [SerializeField]
    private int barrelCount = 2;

    private float nextFireTime;

    public override int Fire(int ammoStock)
    {
        if (magazineState.Count == 0)
        {
            return 0;
        }

        float interval = gunStats.fireRate > 0f ? 1f / gunStats.fireRate : 0f;
        if (interval > 0f && Time.time < nextFireTime)
        {
            return 0;
        }

        nextFireTime = Time.time + interval;

        int firedCount = 0;
        for (int i = 0; i < barrelCount; i++)
        {
            if (magazineState.Count == 0)
            {
                break;
            }

            var bullet = magazineState.Pop();
            while (bullet.isEmpty)
            {
                if (magazineState.Count == 0)
                {
                    bullet = null;
                    break;
                }

                bullet = magazineState.Pop();
            }

            if (bullet == null)
            {
                break;
            }

            Camera cam = Camera.main;
            Vector3 shotDirection = GetShotDirection(cam.transform);

            var bulletObj = Instantiate(bulletPrefab, cam.transform.position, source.transform.rotation);
            var bs = bulletObj.GetComponent<BulletScript>();
            bs.Initialize(gunStats.bulletLifeTime, shotDirection, gunStats.bulletSpeed);

            NotifyFiredBullet(bullet);
            firedCount++;

            // raycast (only hits Zombie and Default layer)
            Ray ray = new(cam.transform.position, shotDirection);
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
                        targets = new Queue<GameObject>(new[] { hit.collider.gameObject }),
                        position = hit.point,
                        sourceBullet = bullet
                    };

                    NotifyHitTarget(context);
                }
                NotifyHitSomething(ray, hit);
                bs.SetLifetime(0.5f); // rough hack to make bullet disappear quickly after hit
            }
        }

        return firedCount;
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
        int bulletsToLoad = Mathf.Min(ammoStock, magazineBlueprint.bullets.Length, barrelCount);

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