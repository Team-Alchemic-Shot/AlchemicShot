using UnityEngine;

public class AudioManager
{
    public static void PlayGunfire(GunDefinition definition, GameObject player)
    {
        if (definition.fx.shootSound != null)
        {
            AudioSource.PlayClipAtPoint(
                definition.fx.shootSound,
                player.transform.position,
                definition.fx.shootSoundVolume);
        }
    }

    public static void PlayReload(GunDefinition definition, GameObject player)
    {
        if (definition.fx.reloadSound != null)
        {
            AudioSource.PlayClipAtPoint(
                definition.fx.reloadSound,
                player.transform.position,
                definition.fx.reloadSoundVolume);
        }
    }

    public static void Subscribe(Gun gun)
    {
        
    }
}