using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("HUD Elements")]
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text scoreText;

    // Cached references to save CPU cycles
    private Health playerHealth;
    private float lastKnownHealth = -1f;

    void Start()
    {
        // Find the player ONCE at the start of the game
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
        }
        else
        {
            Debug.LogError("UIManager: Could not find the Player in the scene!");
        }
    }

    void Update()
    {
        // Only update the UI text if the health value has actually changed
        if (playerHealth != null && playerHealth.CurrentHealth != lastKnownHealth)
        {
            lastKnownHealth = playerHealth.CurrentHealth;
            // Mathf.CeilToInt ensures we display clean whole numbers (e.g., 99 instead of 98.4)
            healthText.text = Mathf.CeilToInt(lastKnownHealth).ToString(); 
        }
        
        // TODO: Apply the same caching logic for the score variable here
    }
}