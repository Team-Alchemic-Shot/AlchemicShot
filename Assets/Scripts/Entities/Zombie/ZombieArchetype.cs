using UnityEngine;

[CreateAssetMenu(fileName = "Zombie", menuName = "Zombies/Zombie")]
public class ZombieArchetype : ScriptableObject
{
    [Header("Targeting")]
    public float detectionRadius = 20f;
    public float refreshInterval = 0.5f;
    public int maxTargetColliders = 32;

    [Header("Speed (AI intent)")]
    public float baseSpeed = 2f;
    public float chaseSpeed = 6f;

    [Header("Wander")]
    public float wanderRadius = 10f;
    public float wanderInterval = 2f;

    [Header("Chase")]
    public float repathInterval = 0.25f; // how often to update the path while chasing
    public float orbitRadius = 1.5f; // when close, zombies spread
    public float orbitAngularSpeed = 0.8f;
    public float chaseStoppingDistance = 1.25f; // stop distance, breathing room.

    //TODO: maybe? in zombieclimb, kind of works
    [Header("Climb")]
    public bool canclimb = false;
    [Range(0f, 1f)]
    public float climbChance = 0.15f;
    public float climbHeight = 0.75f;
    public float climbDuration = 0.9f;
    public float climbCooldown = 2.5f;
}