using System;
using UnityEngine;

public class Health : EntityComponent, IDamageable
{
    [SerializeField]
    private float maxHealth = 100f;
    [SerializeField]
    private bool destroyOnDeath = true;

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

        CurrentHealth = Mathf.Max(0f, CurrentHealth - info.amount);
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
}
