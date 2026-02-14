using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : MonoBehaviour
{
    [SerializeField] private float damage;
    private float lastTimeAttacked;
    public float cooldown = 1;
    bool cooldownActive => Time.time - lastTimeAttacked < cooldown;

    public void OnTriggerStay(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            if (!cooldownActive)
            {
                PlayerHealth health = GameObject.FindWithTag("Player").GetComponent<PlayerHealth>();
                attack(damage, health);
            }
        }
    }

    void attack(float damage, PlayerHealth health)
    {
        lastTimeAttacked = Time.time;
        health.TakeDamage(damage);
    }
}
