using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class RadialRevolverUI : MagazineUI
{
    private static readonly WaitForSeconds FlashDelay = new(0.15f);

    [Header("Core References")]
    public Gun playerGun;
    public ElementDatabase elementDatabase;
    
    [Header("UI Visuals")]
    [Tooltip("Order them: 0=Top(Red), 1=Right(Green), 2=Bottom(Blue), 3=Left(Cyan)")]
    public Image[] elementIcons;
    public TextMeshProUGUI draftingTextDisplay; 

    private int currentChamberIndex = 0;
    private int maxChambers;
    private Color[] originalColors;

    private Dictionary<int, string> draftedElements = new()
    {
        { 0, "Fire" }, // Red
        { 1, "Earth" }, // Green
        { 2, "Water" }, // Blue
        { 3, "Air" }  // Cyan
    };

    void Start()
    {
        // Auto-find the gun
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerGun = player.GetComponentInChildren<Gun>(); // TODO get from inventory once merged with reactions or dev

        // Store the original colors (Red, Green, Blue, Cyan) so we can restore them after a flash
        originalColors = new Color[elementIcons.Length];
        for(int i = 0; i < elementIcons.Length; i++)
        {
            originalColors[i] = elementIcons[i].color;
        }

        maxChambers = playerGun.gunDefinition.stats.magazineSize;
        
        UpdateDraftingUI();
    }

    void Update()
    {
        if (playerGun == null) return;

        // Listen for hotkeys if the cylinder isn't full yet
        if (currentChamberIndex < maxChambers)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) DraftElement(0); // 1 Key
            if (Input.GetKeyDown(KeyCode.Alpha2)) DraftElement(1); // 2 Key
            if (Input.GetKeyDown(KeyCode.Alpha3)) DraftElement(2); // 3 Key
            if (Input.GetKeyDown(KeyCode.Alpha4)) DraftElement(3); // 4 Key
        }

        // Press 'C' to clear mistakes
        if (Input.GetKeyDown(KeyCode.C)) ClearDraft();
    }

    private void DraftElement(int index)
    {
        MagazineBlueprint blueprint = playerGun.GetMagazineBlueprint();
        if (index >= elementDatabase.elements.Length) return; 

        Element selectedElement = elementDatabase.GetElementByName(draftedElements[index]);

        if (blueprint.bullets[currentChamberIndex] == null)
        {
            blueprint.bullets[currentChamberIndex] = new BulletData();
        }

        blueprint.bullets[currentChamberIndex].element = selectedElement;
        currentChamberIndex = ++currentChamberIndex % maxChambers; // wrap

        // Trigger the visual pop effect
        StartCoroutine(FlashIconRoutine(index));
        UpdateDraftingUI();
    }

    private IEnumerator FlashIconRoutine(int index)
    {
        // Make the icon turn white and scale up slightly
        elementIcons[index].color = Color.white;
        elementIcons[index].rectTransform.localScale = Vector3.one * 1.2f;
        
        yield return FlashDelay;
        
        // Return to its original color and size
        elementIcons[index].color = originalColors[index];
        elementIcons[index].rectTransform.localScale = Vector3.one;
    }

    private void ClearDraft()
    {
        MagazineBlueprint blueprint = playerGun.GetMagazineBlueprint();
        for (int i = 0; i < blueprint.bullets.Length; i++)
        {
            blueprint.bullets[i] = null;
        }
        currentChamberIndex = 0;
        UpdateDraftingUI();
    }

    private void UpdateDraftingUI()
    {
        if (draftingTextDisplay == null) return;

        string displayText = "<b>NEXT SHOTS:</b>\n";
        MagazineBlueprint blueprint = playerGun.GetMagazineBlueprint();

        for (int i = 0; i < maxChambers; i++)
        {
            bool isCursor = i == currentChamberIndex;
            if (blueprint.bullets[i] != null && blueprint.bullets[i].element != null)
            {
                string hexColor = ColorUtility.ToHtmlStringRGB(blueprint.bullets[i].element.elementColor);
                displayText += $"<color=#{hexColor}>[ {blueprint.bullets[i].element.elementName} ]</color>{(isCursor ? " <" : "")}\n";
            }
            else
            {
                displayText += $"<color=#888888><i>[ Empty ]</i></color>{(isCursor ? " <" : "")}\n";
            }
        }
        draftingTextDisplay.text = displayText;
    }

    public override void Reload() 
    { 
        currentChamberIndex = 0; 
        ClearDraft(); 
    }
    
    public override void Shoot() { }
}