using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central manager for Call of Duty style zombie rounds.
/// Tracks round progression, zombie counts, and difficulty scaling.
/// Coordinates with spawn points and map unlockers.
/// </summary>
public class ZombieRoundManager : MonoBehaviour
{
    [Serializable]
    public class RoundConfig
    {
        [Tooltip("Base number of zombies for this round")]
        public int zombieCount = 6;
        
        [Tooltip("Time between individual zombie spawns")]
        public float spawnInterval = 0.8f;
        
        [Tooltip("Multiplier for zombie health/damage")]
        [Range(1f, 3f)]
        public float difficultyMultiplier = 1f;
    }

    public static ZombieRoundManager Instance { get; private set; }

    [Header("Round Configuration")]
    [SerializeField]
    private RoundConfig[] roundConfigs;
    
    [SerializeField]
    [Range(0f, 5f)]
    private float difficultyIncreasePerRound = 0.15f;
    
    [SerializeField]
    private float delayBetweenRounds = 10f;

    [Header("References")]
    [SerializeField]
    private GameObject player;

    // Events
    public event Action<int> OnRoundStarted; // round number (1-indexed)
    public event Action<int> OnRoundEnded; // round number (1-indexed)
    public event Action<int> OnZombieCountChanged; // remaining zombie count
    public event Action<int> OnRoundUnlock; // unlock map areas at this round
    public event Action<string> OnStatusUpdate;

    // State
    private int currentRound = 0;
    private int zombiesInRound = 0;
    private int zombiesKilled = 0;
    private int zombiesSpawned = 0;
    private bool roundInProgress = false;
    private List<ZombieSpawnPoint> spawnPoints = new();
    private List<MapLocation> mapLocations = new();
    private Dictionary<int, List<int>> unlocksPerRound = new(); // round -> list of area IDs
    private HashSet<int> unlockedLocationIds = new() { 0 }; // Start unlocked locations (0 = start area)

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Auto-find player if not assigned
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }

        // Find all spawn points and map locations in scene
        FindGameplayElements();

        // Start the first round
        StartNextRound();
        OnZombieCountChanged?.Invoke(GetZombiesLeft());
    }

    private void FindGameplayElements()
    {
        spawnPoints.Clear();
        spawnPoints.AddRange(FindObjectsOfType<ZombieSpawnPoint>());

        mapLocations.Clear();
        mapLocations.AddRange(FindObjectsOfType<MapLocation>());

        OnStatusUpdate?.Invoke($"Found {spawnPoints.Count} spawn points and {mapLocations.Count} map locations");
    }

    /// <summary>
    /// Registers a spawn point to be managed by this round manager.
    /// Called by ZombieSpawnPoint during its initialization.
    /// </summary>
    public void RegisterSpawnPoint(ZombieSpawnPoint spawnPoint)
    {
        if (!spawnPoints.Contains(spawnPoint))
        {
            spawnPoints.Add(spawnPoint);
        }
    }

    /// <summary>
    /// Registers a map location to be managed by this round manager.
    /// </summary>
    public void RegisterMapLocation(MapLocation location)
    {
        if (!mapLocations.Contains(location))
        {
            mapLocations.Add(location);
        }
    }

    /// <summary>
    /// Registers an unlock that occurs at a specific round.
    /// </summary>
    public void RegisterRoundUnlock(int round, int locationId)
    {
        if (!unlocksPerRound.ContainsKey(round))
        {
            unlocksPerRound[round] = new List<int>();
        }
        
        if (!unlocksPerRound[round].Contains(locationId))
        {
            unlocksPerRound[round].Add(locationId);
        }
    }

    /// <summary>
    /// Starts the next round sequence.
    /// </summary>
    public void StartNextRound()
    {
        if (roundInProgress)
        {
            OnStatusUpdate?.Invoke("Round already in progress");
            return;
        }

        zombiesSpawned = 0;
        zombiesKilled = 0;
        roundInProgress = true;
        currentRound++;

        OnRoundStarted?.Invoke(currentRound);
        OnStatusUpdate?.Invoke($"Round {currentRound} started!");

        // Calculate zombie count for this round
        CalculateZombieCount();
        OnZombieCountChanged?.Invoke(GetZombiesLeft());

        // Activate valid spawn points for this round
        int activeSpawnPoints = 0;
        foreach (var spawnPoint in spawnPoints)
        {
            // Only activate if the spawn point is in an unlocked area
            if (unlockedLocationIds.Contains(spawnPoint.GetLocationId()))
            {
                spawnPoint.ActivateForRound(currentRound);
                activeSpawnPoints++;
            }
        }
        
        OnStatusUpdate?.Invoke($"Round {currentRound} started with {activeSpawnPoints} active spawn points");
    }

    /// <summary>
    /// Attempts to reserve a spawn slot for this round.
    /// Returns false when no spawns remain.
    /// </summary>
    public bool TryRegisterSpawn()
    {
        if (!roundInProgress)
        {
            return false;
        }

        if (zombiesSpawned >= zombiesInRound)
        {
            return false;
        }

        zombiesSpawned++;
        OnZombieCountChanged?.Invoke(GetZombiesLeft());
        return true;
    }

    /// <summary>
    /// Called by zombies or spawn points when a zombie dies.
    /// </summary>
    public void OnZombieDied(Health _)
    {
        zombiesKilled = Mathf.Min(zombiesKilled + 1, zombiesInRound);
        OnZombieCountChanged?.Invoke(GetZombiesLeft());

        // If all zombies are dead, end the round
        if (roundInProgress && zombiesSpawned >= zombiesInRound && zombiesKilled >= zombiesInRound)
        {
            EndRound();
        }
    }

    /// <summary>
    /// Ends the current round and schedules the next one.
    /// </summary>
    private void EndRound()
    {
        roundInProgress = false;

        // Unlock map areas for this completed round
        if (unlocksPerRound.TryGetValue(currentRound, out List<int> locationIds))
        {
            foreach (int locationId in locationIds)
            {
                UnlockMapArea(locationId);
            }
            OnRoundUnlock?.Invoke(currentRound);
        }

        OnRoundEnded?.Invoke(currentRound);
        OnStatusUpdate?.Invoke($"Round {currentRound} complete! Next round in {delayBetweenRounds}s...");

        Invoke(nameof(StartNextRound), delayBetweenRounds);
    }

    /// <summary>
    /// Calculates total zombie count for current round based on spawn points.
    /// </summary>
    private void CalculateZombieCount()
    {
        if (currentRound > roundConfigs.Length)
        {
            // Use last config and scale difficulty
            var lastConfig = roundConfigs[^1];
            zombiesInRound = lastConfig.zombieCount;
        }
        else
        {
            var config = roundConfigs[currentRound - 1];
            zombiesInRound = config.zombieCount;
        }
    }

    /// <summary>
    /// Gets the configuration for the current round.
    /// </summary>
    public RoundConfig GetCurrentRoundConfig()
    {
        if (currentRound > roundConfigs.Length)
        {
            return roundConfigs[roundConfigs.Length - 1];
        }
        return roundConfigs[currentRound - 1];
    }

    /// <summary>
    /// Gets difficulty multiplier scaled by round progression.
    /// </summary>
    public float GetDifficultyMultiplier()
    {
        var config = GetCurrentRoundConfig();
        float baseMultiplier = config.difficultyMultiplier;
        float scaledMultiplier = baseMultiplier + ((currentRound - 1) * difficultyIncreasePerRound);
        return scaledMultiplier;
    }

    /// <summary>
    /// Unlocks a map area by ID.
    /// </summary>
    private void UnlockMapArea(int locationId)
    {
        if (!unlockedLocationIds.Contains(locationId))
        {
            unlockedLocationIds.Add(locationId);
        }

        bool unlockedAny = false;
        foreach (var location in mapLocations)
        {
            if (location.GetLocationId() == locationId)
            {
                location.Unlock();
                unlockedAny = true;
            }
        }

        if (!unlockedAny)
        {
            OnStatusUpdate?.Invoke($"No map locations found for locationId {locationId}");
        }
    }

    /// <summary>
    /// Gets current round number (1-indexed).
    /// </summary>
    public int GetCurrentRound() => currentRound;

    /// <summary>
    /// Gets zombie count remaining in current round.
    /// </summary>
    public int GetZombiesLeft() => Mathf.Max(0, zombiesInRound - zombiesKilled);

    /// <summary>
    /// Checks if round is in progress.
    /// </summary>
    public bool IsRoundInProgress() => roundInProgress;

    /// <summary>
    /// Gets the current player object.
    /// </summary>
    public GameObject GetPlayer() => player;
}
