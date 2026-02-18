using System;
using UnityEngine;

/// <summary>
/// Represents a lockable area of the map.
/// Can be unlocked after reaching a certain round.
/// Disables/enables colliders and visual elements when locked/unlocked.
/// </summary>
public class MapLocation : MonoBehaviour
{
    [Header("Location Configuration")]
    [SerializeField]
    private int locationId = 0;
    
    [SerializeField]
    [Tooltip("Round number at which this location unlocks (0 = always unlocked)")]
    private int unlockRound = 0;
    
    [SerializeField]
    [Tooltip("Cost in points to unlock (optional, for future implementation)")]
    private int unlockCost = 0;

    [Header("Locked State Configuration")]
    [SerializeField]
    private Collider[] blockingColliders;
    
    [SerializeField]
    private Renderer[] lockVisuals;
    
    [SerializeField]
    private Color lockedColor = Color.red;
    
    [SerializeField]
    private Color unlockedColor = Color.white;

    [SerializeField]
    [Tooltip("Unlock particles or effects")]
    private ParticleSystem unlockEffect;

    [Header("Debug")]
    [SerializeField]
    private bool startUnlocked = false;

    [Header("Door Animation")]
    [SerializeField]
    private Transform doorHinge; // Assign the door GameObject or its pivot in Inspector

    [SerializeField]
    private float openAngle = 90f; // Angle to swing open

    [SerializeField]
    private float openSpeed = 2f; // How fast the door opens

    private bool isOpening = false;
    private Quaternion targetRotation;

    // State
    private bool isLocked = true;
    private ZombieRoundManager roundManager;

    public event Action<int> OnLocationUnlocked; // location ID

    private void Start()
    {
        roundManager = ZombieRoundManager.Instance;

        if (roundManager != null)
        {
            roundManager.RegisterMapLocation(this);

            // Register unlock at specific round if configured
            if (unlockRound > 0)
            {
                roundManager.RegisterRoundUnlock(unlockRound, locationId);
            }
        }

        // Initialize locked/unlocked state
        if (startUnlocked || unlockRound == 0)
        {
            Unlock();
        }
        else
        {
            Lock();
        }
    }

    /// <summary>
    /// Locks this location, blocking player passage.
    /// </summary>
    public void Lock()
    {
        isLocked = true;

        // Enable blocking colliders
        foreach (var collider in blockingColliders)
        {
            if (collider != null)
                collider.enabled = true;
        }

        // Set locked visuals
        foreach (var renderer in lockVisuals)
        {
            if (renderer != null)
            {
                foreach (var material in renderer.materials)
                {
                    material.color = lockedColor;
                }
            }
        }
    }

    /// <summary>
    /// Unlocks this location, allowing player passage.
    /// </summary>
    public void Unlock()
    {
        isLocked = false;

        // Disable blocking colliders
        foreach (var collider in blockingColliders)
        {
            if (collider != null)
                collider.enabled = false;
        }

        // Set unlocked visuals
        foreach (var renderer in lockVisuals)
        {
            if (renderer != null)
            {
                foreach (var material in renderer.materials)
                {
                    material.color = unlockedColor;
                }
            }
        }

        // Play unlock effect
        if (unlockEffect != null)
        {
            unlockEffect.Play();
        }

        // Start opening the door
        if (doorHinge != null)
        {
            targetRotation = Quaternion.Euler(0, openAngle, 0) * doorHinge.localRotation;
            isOpening = true;
        }

        OnLocationUnlocked?.Invoke(locationId);
    }

    /// <summary>
    /// Checks if location is currently locked.
    /// </summary>
    public bool IsLocked() => isLocked;

    /// <summary>
    /// Gets the location ID.
    /// </summary>
    public int GetLocationId() => locationId;

    /// <summary>
    /// Gets the round at which this location unlocks.
    /// </summary>
    public int GetUnlockRound() => unlockRound;

    private void OnDrawGizmosSelected()
    {
        // Draw location bounds
        Gizmos.color = isLocked ? Color.red : Color.green;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 5f);
    }
    private void Update()
    {
        if (isOpening && doorHinge != null)
        {
            doorHinge.localRotation = Quaternion.Lerp(doorHinge.localRotation, targetRotation, Time.deltaTime * openSpeed);
            if (Quaternion.Angle(doorHinge.localRotation, targetRotation) < 1f)
            {
                doorHinge.localRotation = targetRotation;
                isOpening = false;
            }
        }
    }
}
