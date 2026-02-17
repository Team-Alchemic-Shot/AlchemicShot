using UnityEngine;

public static class ElementDirection
{
    /// <summary>
    /// returns a biased direction biased around preferred direction.
    /// results stay close to preferredDir.
    /// larger maxAngle = wider spread
    /// </summary>
    public static Vector3 BiasedDirection(Vector3 preferredDir, float maxAngle)
    {
        // Flatten to XZ plane
        preferredDir.y = 0f;

        if (preferredDir.sqrMagnitude < 0.0001f)
        {
            return Vector3.forward;
        }

        preferredDir.Normalize();


        float angleOffset = Random.Range(-1f, 1f);

        // Bias toward 0 
        angleOffset = Mathf.Sign(angleOffset) * angleOffset * angleOffset;

        // scale maxAngle degrees
        angleOffset *= maxAngle;

        // rotate around y
        return RotateY(preferredDir, angleOffset);
    }

    private static Vector3 RotateY(Vector3 dir, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;

        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        float x = dir.x * cos - dir.z * sin;
        float z = dir.x * sin + dir.z * cos;

        return new Vector3(x, 0f, z).normalized;
    }
}
