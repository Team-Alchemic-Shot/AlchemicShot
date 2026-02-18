using UnityEngine;

public class AudioManager
{
    public static void PlayGunfire(GunDefinition definition, GameObject player)
    {
        if (definition.fx.shootSound != null)
        {
            SoundManager.Instance.PlaySound3D(
                definition.fx.shootSound,
                player.transform.position);
        }
    }

    public static void PlayReload(GunDefinition definition, GameObject player)
    {
        if (definition.fx.reloadSound != null)
        {
            SoundManager.Instance.PlaySound3D(
                definition.fx.reloadSound,
                player.transform.position);
        }
    }
}