using UnityEngine;

[RequireComponent(typeof(ZombieTargeting))]
public class ZombieAttack : MonoBehaviour
{
    [SerializeField]
    private float attackRange = 1.5f;
    [SerializeField]
    private float attackInterval = 1f;
    [SerializeField]
    private float damageAmount = 10f;

    private float attackTimer;
    private ZombieTargeting targeting;

    private void Awake()
    {
        targeting = GetComponent<ZombieTargeting>();
    }

    private void Update()
    {
        attackTimer -= Time.deltaTime;
        if (attackTimer > 0f)
        {
            return;
        }

        if (targeting == null || targeting.CurrentTarget == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, targeting.CurrentTarget.position);
        if (distance > attackRange)
        {
            return;
        }

        var damageable = targeting.CurrentTarget.GetComponentInParent<IDamageable>();
        if (damageable == null)
        {
            return;
        }

        attackTimer = attackInterval;
        damageable.ApplyDamage(new DamageInfo(damageAmount, gameObject, targeting.CurrentTarget.position));
    }
}
