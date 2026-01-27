using UnityEngine;

public struct DamageInfo
{
    public float amount;
    public GameObject source;
    public Vector3 position;

    public DamageInfo(float amount, GameObject source, Vector3 position)
    {
        this.amount = amount;
        this.source = source;
        this.position = position;
    }
}
