using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BulletScript : MonoBehaviour
{
    private float lifetime = 10f;
    private float lifeTimer = 0f;
    private Rigidbody rb;
    private GameObject currentVfx;

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

    public void Initialize(float lifetime, Vector3 direction, float speed, BulletData bulletData)
    {
        this.lifetime = lifetime;
        rb.velocity = direction.normalized * speed;

        if (bulletData != null && bulletData.element != null)
        {
            ApplyElementVisuals(bulletData.element);
        }
    }

    private void ApplyElementVisuals(Element element)
    {
        // Instantiate specific projectile VFX if available
        if (element.projectileVfxPrefab != null)
        {
            if (currentVfx != null)
            {
                Destroy(currentVfx);
            }
            currentVfx = Instantiate(element.projectileVfxPrefab, transform.position, transform.rotation, transform);
        }

        // Fallback or additional: color the mesh and trail
        if (TryGetComponent<MeshRenderer>(out var meshRenderer))
        {
            meshRenderer.material.color = element.elementColor;
            meshRenderer.material.SetColor("_EmissionColor", element.elementColor * 2f);
        }

        if (TryGetComponent<TrailRenderer>(out var trailRenderer))
        {
            trailRenderer.startColor = element.elementColor;
            trailRenderer.endColor = new Color(element.elementColor.r, element.elementColor.g, element.elementColor.b, 0f);
        }
    }

    public void SetLifetime(float lifetime)
    {
        this.lifetime = lifetime;
    }
}