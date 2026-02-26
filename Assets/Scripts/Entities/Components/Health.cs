using System;
using UnityEngine;

public class Health : EntityComponent, IDamageable
{
    [SerializeField]
    private float maxHealth = 100f;
    [SerializeField]
    private bool destroyOnDeath = true;

    [SerializeField, HideInInspector] private float knockbackForce = 10f;
    [SerializeField, HideInInspector] private float knockbackUpwardForce = 0.5f;

    private float weaknessMultiplier = 0f;

    public float CurrentHealth { get; private set; }
    public float MaxHealth => maxHealth;

    

    public event Action<Health> OnDamaged;
    public event Action<Health> OnDeath;

    protected override void Awake()
    {
        base.Awake();
        CurrentHealth = maxHealth;
    }

    public void ApplyDamage(DamageInfo info)
    {
        if (CurrentHealth <= 0f)
        {
            return;
        }
        Debug.Log($"[Damage] {name} took {info.amount} from {(info.source ? info.source.name : "NULL")} at {info.position}\n{Environment.StackTrace}", gameObject);        float scaledDamage = info.amount * (1f + Mathf.Max(0f, weaknessMultiplier));
        // Debug.Log($"base damage={info.amount} weak={weaknessMultiplier} scaled={scaledDamage}");

        CurrentHealth = Mathf.Max(0f, CurrentHealth - scaledDamage);
        // Debug.Log($"health = {CurrentHealth}");

        if (CompareTag("Player"))
        {
            ApplyKnockback(info);
        }

        OnDamaged?.Invoke(this);

        

        if (CurrentHealth <= 0f)
        {
            OnDeath?.Invoke(this);
            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }
    }


    private void ApplyKnockback(DamageInfo info)
    {
        if (info.source == null)
        {
            return;
        }

        var rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            return;
        }

        // default to player health knockback values
        float force = knockbackForce;
        float up = knockbackUpwardForce;

        // if the attacker is a zombie use archetype values
        if (info.source.TryGetComponent<ZombieAttack>(out var zombieAttack))
        {
            force = zombieAttack.KnockbackForce;
            up = zombieAttack.KnockbackUpwardForce;
        }

        // push away from damage source
        Vector3 dir = (transform.position - info.source.transform.position);
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
        {
            return;
        }

        dir.Normalize();

        Vector3 impulse = dir * force;
        impulse.y = up;

        rb.AddForce(impulse, ForceMode.Impulse);
    }

    public void Heal(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
    }


    
    public void ApplyWeakness(float multiplier)
    {
        weaknessMultiplier += multiplier;
    } 

    public void ScaleMaxHealth(float multiplier, bool refillToMax = true)
    {
        if (multiplier <= 0f)
        {
            return;
        }

        maxHealth = Mathf.Max(1f, maxHealth * multiplier);

        if (refillToMax)
        {
            CurrentHealth = maxHealth;
        }
        else
        {
            CurrentHealth = Mathf.Min(CurrentHealth * multiplier, maxHealth);
        }
    }
}
