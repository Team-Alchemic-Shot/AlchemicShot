using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAnimations : MonoBehaviour
{
    private Animator animator;
    private Animation anim;
    private bool attacking;
    private bool dying;
    public float attackAnimLength = 1.667f;
    public float deathAnimLength = 1.042f;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        anim = GetComponentInChildren<Animation>();
    }

    private void OnEnable()
    {
        var movement = GetComponent<ZombieTargeting>();
        var attack = GetComponent<ZombieAttack>();
        var health = GetComponent<Health>();

        // subscribe to changes in zombie behavior
        movement.walking += OnWalkStart;
        movement.running += OnRunStart;
        attack.attacking += OnAttack;
        health.OnDeath += OnDeath;
    }

    private void OnDisable()
    {
        var movement = GetComponent<ZombieTargeting>();
        var attack = GetComponent<ZombieAttack>();
        var health = GetComponent<Health>();

        movement.walking -= OnWalkStart;
        movement.running -= OnRunStart;
        attack.attacking -= OnAttack;
        health.OnDeath -= OnDeath;
    }

    void OnWalkStart()
    {
        if (!attacking)
        {
            anim.Play("Walk");
        }
    }

    void OnRunStart()
    {
        if (!attacking && !dying)
        {
            anim.Play("Run");
        }
    }

    void OnAttack()
    {
        if (!attacking && !dying)
        {
            StartCoroutine(PlayAttackAnimation(attackAnimLength));
        }
    }

    void OnDeath(Health health)
    {
        dying = true;
        anim.Stop();
        anim.Play("Death");
    }

    private IEnumerator PlayAttackAnimation(float timeActive)
    {
        attacking = true;
        int rand = Random.Range(0, 2);
        if (rand == 0)
        {
            anim.Play("Attack1");
        }
        else
        {
            anim.Play("Attack2");
        }
        yield return new WaitForSeconds(timeActive);
        attacking = false;
    }
}
