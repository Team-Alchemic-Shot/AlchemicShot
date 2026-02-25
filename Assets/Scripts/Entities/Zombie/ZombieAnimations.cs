using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    private Animator animator;
    private Animation anim;
    private bool attacking;
    private float attackAnimLength = 1.667f;

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

        // subscribe to changes in zombie behavior
        movement.walking += OnWalkStart;
        movement.running += OnRunStart;
        attack.attacking += OnAttack;
    }

    private void OnDisable()
    {
        var movement = GetComponent<ZombieTargeting>();
        var attack = GetComponent<ZombieAttack>();

        movement.walking -= OnWalkStart;
        movement.running -= OnRunStart;
        attack.attacking -= OnAttack;
    }

    void OnWalkStart()
    {
        //animator.Play("Walk");
        anim.Play("Walk");
        Debug.Log("zombie started walking");
    }

    void OnRunStart()
    {
        if (!attacking)
        {
            anim.Play("Run");
            Debug.Log("zombie started running");
        }
    }

    void OnAttack()
    {
        if (!attacking)
        {
            StartCoroutine(PlayAttackAnimation(attackAnimLength));
        }
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
