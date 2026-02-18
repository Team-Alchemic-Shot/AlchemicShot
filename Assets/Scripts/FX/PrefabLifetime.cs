using UnityEngine;

public class PrefabLifetime : MonoBehaviour
{
    private float lifetime = 1f;
    private float currentTime = 0f;
    private Animator animator;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        lifetime = animator.GetCurrentAnimatorStateInfo(0).length;
    }

    void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}