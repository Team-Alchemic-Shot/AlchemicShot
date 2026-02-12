[System.Serializable]
public class MagazineBlueprint
{
    public BulletData[] bullets;
    public MagazineBlueprint(int size)
    {
        bullets = new BulletData[size];
    }
}