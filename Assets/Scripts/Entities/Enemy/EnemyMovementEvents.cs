using UnityEngine;

public class EnemyMovementEvents : MonoBehaviour
{
    private Animator animator;
    
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        var direction = GetComponent<MovementDirection>();
        direction.movingRight += OnWalkRightStart;
        direction.movingLeft += OnWalkLeftStart;
        direction.stopped += OnWalkStop;
    }

    private void OnDisable()
    {
        var direction = GetComponent<MovementDirection>();
        direction.movingRight -= OnWalkRightStart;
        direction.movingLeft -= OnWalkLeftStart;
        direction.stopped -= OnWalkStop;
    }

    void OnWalkRightStart()
    {
        if (gameObject.CompareTag("MeleeEnemy")) {
            animator.Play("MeleeEnemy_move_right");
        } 
        else if (gameObject.CompareTag("RangedEnemy"))
        {
            animator.Play("RangedEnemy_move_right");
        }
    }

    void OnWalkLeftStart()
    {
        if (gameObject.CompareTag("MeleeEnemy"))
        {
            animator.Play("MeleeEnemy_move_left");
        }
        else if (gameObject.CompareTag("RangedEnemy"))
        {
            animator.Play("RangedEnemy_move_left");
        }
    }

    void OnWalkStop()
    {
        if (gameObject.CompareTag("MeleeEnemy"))
        {
            animator.Play("MeleeEnemy_idle");
        }
        else if (gameObject.CompareTag("RangedEnemy"))
        {
            animator.Play("RangedEnemy_idle");
        }
    }
}
