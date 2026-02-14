using System;
using UnityEngine;

public class Health : EntityComponent, IDamageable
{
    [SerializeField]
    private float maxHealth = 100f;
    [SerializeField]
    private bool destroyOnDeath = true;

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

        float scaledDamage = info.amount * (1f + Mathf.Max(0f, weaknessMultiplier));
        Debug.Log($"base damage={info.amount} weak={weaknessMultiplier} scaled={scaledDamage}");

        CurrentHealth = Mathf.Max(0f, CurrentHealth - scaledDamage);
        Debug.Log($"health = {CurrentHealth}");
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
        Debug.Log($"ApplyWeakness({multiplier}) -> weakness now {weaknessMultiplier}\n{Environment.StackTrace}");

    } 
}
