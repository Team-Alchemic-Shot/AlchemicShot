using UnityEngine;

public static class GunRecoil
{
    public static void ApplyRecoil(BulletData _, GunDefinition definition, GameObject player)
    {
        if (player == null || definition == null)
        {
            return;
        }

        var gunLook = player.GetComponentInChildren<GunLook>();
        if (gunLook == null)
        {
            return;
        }

        float recoil = definition.stats.recoil;
        if (recoil <= 0f)
        {
            return;
        }

        float pitchKick = -Mathf.Abs(recoil);
        float yawKick = Random.Range(-recoil * 0.25f, recoil * 0.25f);

        gunLook.transform.Rotate(pitchKick, yawKick, 0f, Space.Self);

    }
}
        