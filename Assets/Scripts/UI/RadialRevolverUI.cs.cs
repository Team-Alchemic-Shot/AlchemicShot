using UnityEngine;
using TMPro;

public class RadialRevolverUI : MagazineUI
{

    [Header("Core References")]
    public Gun playerGun;
    public TextMeshProUGUI magazineText; 

    private Inventory inventoryRef;
    private ElementSelector elementSelector;

    void Start()
    {
        // Auto-find the gun
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) 
            playerGun = player.GetComponentInChildren<Gun>(); // TODO get from inventory once merged with reactions or dev

        ChangeGun(playerGun);

        if (player != null && player.TryGetComponent<Inventory>(out var inventory)) 
        {
            inventoryRef = inventory;
            inventoryRef.OnGunChanged += ChangeGun;
        }

        elementSelector = FindObjectOfType<ElementSelector>();
        if (elementSelector != null)
        {
            elementSelector.OnMagazineBlueprintChanged += HandleMagazineBlueprintChanged;
        }

        UpdateUI(playerGun.GetMagazine());
    }

    private void OnDisable()
    {
        if (inventoryRef != null)
        {
            inventoryRef.OnGunChanged -= ChangeGun;
        }

        if (elementSelector != null)
        {
            elementSelector.OnMagazineBlueprintChanged -= HandleMagazineBlueprintChanged;
        }
    }

    private void ChangeGun(Gun newGun)
    {
        if (playerGun != null)
        {
            playerGun.FiredBullet -= OnFired;
            playerGun.Reloaded -= OnReloaded;
        }

        playerGun = newGun;

        if (playerGun != null)
        {
            playerGun.FiredBullet += OnFired;
            playerGun.Reloaded += OnReloaded;
            UpdateUI(playerGun.GetMagazine());
        }
    }

    private void HandleMagazineBlueprintChanged(Gun gun)
    {
        if (gun == null || playerGun == null)
        {
            return;
        }

        if (gun == playerGun)
        {
            ChangeGun(gun);
        }
    }

    public override void OnFired(BulletData data = null)
    {
        UpdateUI(playerGun.GetMagazine());
    }

    public override void OnReloaded(int ammo, MagazineState magazineState)
    {
        UpdateUI(magazineState);
    }

    public void UpdateUI(MagazineState magazineState)
    {
        if (magazineText == null) return;

        string displayText = "<b>NEXT SHOTS:</b>\n";
        int maxChambers = playerGun.gunDefinition.stats.magazineSize;
        var bulletsArray = magazineState.GetBullets().ToArray();
        for (int i = 0; i < maxChambers; i++)
        {
            if (i < bulletsArray.Length && bulletsArray[i] != null && bulletsArray[i].element != null)
            {
                string hexColor = ColorUtility.ToHtmlStringRGB(bulletsArray[i].element.elementColor);
                displayText += $"<color=#{hexColor}>[ {bulletsArray[i].element.elementName} ]</color>\n";
            }
            else
            {
                displayText += $"<color=#888888><i>[ Unimbued ]</i></color>\n";
            }
        }
        magazineText.text = displayText;
    }
}