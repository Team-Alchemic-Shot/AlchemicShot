using UnityEngine;
using TMPro;

public class SimpleAmmoReadout : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI ammoTextDisplay;

    private Gun playerGun;

    void Start()
    {
        // Auto-find the gun so you don't have to hook it up manually
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) 
        {
            playerGun = player.GetComponentInChildren<Gun>();
        }
    }

    void Update()
    {
        if (playerGun == null || ammoTextDisplay == null) return;

        string displayText = "<b>NEXT SHOTS:</b>\n";
        int bulletCount = 0;
        
        // Grab the active bullets from your teammate's MagazineState
        var activeBullets = playerGun.GetMagazine().GetBullets();
        
        foreach (var bullet in activeBullets)
        {
            if (bullet != null && bullet.element != null)
            {
                // Convert the element's color to a hex code for the text
                string hexColor = ColorUtility.ToHtmlStringRGB(bullet.element.elementColor);
                
                // Add the bullet name to the list with its specific color
                displayText += $"<color=#{hexColor}>[ {bullet.element.elementName} ]</color>\n";
                bulletCount++;
            }
        }

        if (bulletCount == 0)
        {
            displayText += "<color=#888888><i>Empty Chamber</i></color>";
        }

        // Push the final string to the screen
        ammoTextDisplay.text = displayText;
    }
}