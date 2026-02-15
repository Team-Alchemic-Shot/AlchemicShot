using UnityEngine;

[CreateAssetMenu(fileName = "GaleBehavior", menuName = "Elements/Behaviors/Tier 1/Gale")]
public class GaleBehavior : ElementBehavior
{
    [Header("Gale Settings")]
    [SerializeField]
    private LayerMask enemyMask = 1;
    [SerializeField]
    private float range = 5f;
    [SerializeField]
    private float scatter = 0.6f;

    public override void Apply(ElementBehaviorContext context)
    {
        if (context.target == null)
        {
            return;
        }
        Debug.Log($"[GALE] Triggered on {context.target.name}");


        // find nearby enemies (including the shot target)
        var center = context.target.transform.position;
        var hits = Physics.OverlapSphere(center, range, enemyMask); // TODO buffer?
        if (hits == null || hits.Length == 0)
        {
            // fallback to just the hit target
            ApplyToTarget(context, center);
            RemoveBehavior(context);
            return;
        }

        foreach (var hit in hits)
        {
            if (hit == null)
            {
                continue;
            }

            // resolve target from collider
            var target = hit.attachedRigidbody != null ? hit.attachedRigidbody.gameObject : hit.gameObject;
            if (target == null)
            {
                continue;
            }

            var targetContext = new ElementBehaviorContext
            {
                instigator = context.instigator,
                target = target,
                position = target.transform.position,
                sourceBullet = context.sourceBullet,
            };

            ApplyToTarget(targetContext, center);
        }

        // remove since this is an instantaneous effect
        RemoveBehavior(context);
    }

    private void ApplyToTarget(ElementBehaviorContext context, Vector3 center)
    {
        if (context.target == null)
        {
            return;
        }

        // apply knockback in a scattered direction away from the center
        if (!context.target.TryGetComponent<Rigidbody>(out var rigidbody))
        {
            return;
        }

        var baseDir = (context.target.transform.position - center);
        baseDir.y = 0f;

        if (baseDir.sqrMagnitude < 0.0001f)
        {
            baseDir = Random.insideUnitSphere;
            baseDir.y = 0f;
        }

        baseDir.Normalize();

        var jitter = Random.insideUnitSphere;
        jitter.y = 0f;
        if (jitter.sqrMagnitude > 0.0001f)
        {
            jitter.Normalize();
        }

        var finalDir = baseDir + jitter * scatter;
        finalDir.y = 0f;
        if (finalDir.sqrMagnitude > 0.0001f)
        {
            finalDir.Normalize();
        }
        else
        {
            finalDir = baseDir;
        }

        rigidbody.AddForce(finalDir * defaultIntensity, ForceMode.Impulse);
    }
}
