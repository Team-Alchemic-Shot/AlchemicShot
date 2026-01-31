using System.Collections.Generic;

public class MagazineState
{
    private readonly Stack<BulletData> bullets = new();

    public int Count => bullets.Count;

    public BulletData Pop()
    {
        return bullets.Pop();
    }

    public void Clear()
    {
        bullets.Clear();
    }

    public void Push(BulletData bullet)
    {
        bullets.Push(bullet);
    }

    public Stack<BulletData> GetBullets()
    {
        return bullets;
    }
}