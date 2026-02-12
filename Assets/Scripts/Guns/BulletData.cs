[System.Serializable]
public class BulletData
{
    public Element element;
    public float baseDamage = 1;

    public bool isEmpty = false;

    public BulletData Clone()
    {
        return new BulletData
        {
            element = element,
            baseDamage = baseDamage,
            isEmpty = isEmpty
        };
    }
}