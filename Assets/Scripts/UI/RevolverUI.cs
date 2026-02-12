using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RevolverUI : MagazineUI
{
    [SerializeField] private Transform bulletPanel;
    [SerializeField] private TextMeshProUGUI bulletTextPrefab;
    private List<BulletData> bullets = new List<BulletData>();
    private Gun gun;

    void Start()
    {
        // 1. Find the gun ONCE at the start to save CPU performance
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            gun = player.GetComponentInChildren<Gun>();
        }
        else
        {
            Debug.LogError("RevolverUI: Could not find Player!");
        }
    }

    // --- THE FIX ---
    // We completely deleted Update() and moved the visual logic here.
    public override void OnFired(BulletData data = null)
    {
        // Step 1: Wipe the UI panel clean so we don't infinitely stack text
        foreach (Transform child in bulletPanel)
        {
            Destroy(child.gameObject);
        }

        // Step 2: Draw the current bullets exactly once
        foreach (var bullet in bullets)
        {
            TextMeshProUGUI text = Instantiate(bulletTextPrefab, bulletPanel);
            
            // Failsafe in case a bullet is empty
            if (bullet != null && bullet.element != null)
            {
                text.text = bullet.element.elementName + " Bullet";
                text.color = bullet.element.elementColor;
            }
            else
            {
                text.text = "Empty Chamber";
                text.color = Color.gray;
            }
        }
    }

    public override void OnReloaded(int ammo, MagazineState magazineState)
    {
        throw new System.NotImplementedException();
    }
}