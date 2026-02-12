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

    public override void Reload()
    {
        if (gun == null) return;

        // Clear out the old local list before adding new ones
        bullets.Clear(); 

        // Grab the fresh magazine from the gun
        foreach (BulletData bullet in gun.GetMagazine().GetBullets())
        {
            bullets.Add(bullet);
        }

        // Update the screen ONLY when reloading
        UpdateUI();
    }

    public override void Shoot()
    {
        if (bullets.Count > 0)
        {
            // Remove the bullet that was just fired
            bullets.RemoveAt(bullets.Count - 1);
            
            // Update the screen ONLY when shooting
            UpdateUI();
        }
    }

    // --- THE FIX ---
    // We completely deleted Update() and moved the visual logic here.
    public override void UpdateUI(BulletData data = null)
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
}