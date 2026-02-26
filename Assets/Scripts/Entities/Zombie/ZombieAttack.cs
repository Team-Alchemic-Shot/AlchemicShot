using System;
using Unity.VisualScripting;
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

    [SerializeField]
    private float knockbackForce = 2f;
    [SerializeField]
    private float knockbackUpwardForce = 0.25f;

    public float KnockbackForce => knockbackForce;
    public float KnockbackUpwardForce => knockbackUpwardForce;
    private float attackTimer;
    private ZombieTargeting targeting;

    public event Action attacking;

    private void Awake()
    {
        targeting = GetComponent<ZombieTargeting>();
    }


    public void ApplyArchetype(ZombieArchetype archetype)
    {
        if (archetype == null)
        {
            return;
        }

        knockbackForce = archetype.knockbackForce;
        knockbackUpwardForce = archetype.knockbackUpwardForce;
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

        attacking?.Invoke();
    }
}
