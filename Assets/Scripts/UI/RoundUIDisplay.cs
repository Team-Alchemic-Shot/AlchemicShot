using TMPro;
using UnityEngine;

/// <summary>
/// Displays round information and zombie count to the player.
/// Subscribes to ZombieRoundManager events for real-time updates.
/// </summary>
public class RoundUIDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField]
    private TextMeshProUGUI roundText;
    
    [SerializeField]
    private TextMeshProUGUI zombieCountText;
    
    [SerializeField]
    private TextMeshProUGUI statusText;

    [Header("Display Settings")]
    [SerializeField]
    private string roundFormat = "ROUND {0}";
    
    [SerializeField]
    private string zombieFormat = "ZOMBIES: {0}";
    
    [SerializeField]
    private bool autoHideStatus = true;
    
    [SerializeField]
    private float statusDisplayDuration = 5f;

    private ZombieRoundManager roundManager;
    private float statusHideTimer = 0f;

    private void Start()
    {
        // Find or get round manager
        roundManager = ZombieRoundManager.Instance;

        if (roundManager == null)
        {
            Debug.LogError("RoundUIDisplay: ZombieRoundManager singleton not found!");
            return;
        }

        // Subscribe to events
        roundManager.OnRoundStarted += HandleRoundStarted;
        roundManager.OnRoundEnded += HandleRoundEnded;
        roundManager.OnZombieCountChanged += HandleZombieCountChanged;
        roundManager.OnStatusUpdate += HandleStatusUpdate;
        roundManager.OnRoundUnlock += HandleRoundUnlock;

        // Initial update
        UpdateRoundDisplay();
        UpdateZombieDisplay();
    }

    private void Update()
    {
        // Auto-hide status message after duration
        if (autoHideStatus && statusText != null)
        {
            statusHideTimer -= Time.deltaTime;
            if (statusHideTimer <= 0f)
            {
                statusText.text = "";
            }
        }
    }

    private void OnDestroy()
    {
        if (roundManager == null)
            return;

        // Unsubscribe from events
        roundManager.OnRoundStarted -= HandleRoundStarted;
        roundManager.OnRoundEnded -= HandleRoundEnded;
        roundManager.OnZombieCountChanged -= HandleZombieCountChanged;
        roundManager.OnStatusUpdate -= HandleStatusUpdate;
        roundManager.OnRoundUnlock -= HandleRoundUnlock;
    }

    private void HandleRoundStarted(int roundNum)
    {
        UpdateRoundDisplay();
        UpdateZombieDisplay();
    }

    private void HandleRoundEnded(int roundNum)
    {
        // Update displays
        UpdateRoundDisplay();
    }

    private void HandleZombieCountChanged(int count)
    {
        UpdateZombieDisplay();
    }

    private void HandleStatusUpdate(string statusMessage)
    {
        if (statusText != null)
        {
            statusText.text = statusMessage;
            statusHideTimer = statusDisplayDuration;
        }
    }

    private void HandleRoundUnlock(int roundNum)
    {
        if (statusText != null)
        {
            statusText.text = $"MAP AREA UNLOCKED!";
            statusHideTimer = statusDisplayDuration;
        }
    }

    private void UpdateRoundDisplay()
    {
        if (roundText != null && roundManager != null)
        {
            int currentRound = roundManager.GetCurrentRound();
            roundText.text = string.Format(roundFormat, currentRound);
        }
    }

    private void UpdateZombieDisplay()
    {
        if (zombieCountText != null && roundManager != null)
        {
            int zombiesRemaining = roundManager.GetZombiesLeft();
            zombieCountText.text = string.Format(zombieFormat, zombiesRemaining);
        }
    }
}
