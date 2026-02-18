using UnityEngine;

public class MeleeEnemy : MonoBehaviour
{
    [SerializeField] private float damage;
    private float lastTimeAttacked;
    public float cooldown = 1;
    private bool CooldownActive => Time.time - lastTimeAttacked < cooldown;

    public void OnTriggerStay(Collider col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            if (!CooldownActive)
            {
                Health health = GameObject.FindWithTag("Player").GetComponent<Health>();
                attack(damage, health);
            }
        }
    }

    void attack(float damage, Health health)
    {
        lastTimeAttacked = Time.time;
        health.ApplyDamage(new DamageInfo(damage, gameObject, transform.position));
    }
}
