using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BulletScript : MonoBehaviour
{
    private float lifetime = 10f;
    private float lifeTimer = 0f;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void Initialize(float lifetime, Vector3 direction, float speed)
    {
        this.lifetime = lifetime;
        rb.velocity = direction.normalized * speed;
    }
}