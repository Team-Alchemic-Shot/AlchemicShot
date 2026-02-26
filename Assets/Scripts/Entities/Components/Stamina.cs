using UnityEngine;

public class Stamina : MonoBehaviour
{
    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float regenPerSecond = 20f;

    [Header("Drain")]
    [SerializeField] private float sprintDrainPerSecond = 25f;
    [SerializeField] private float regenDelayAfterDrain = 0.35f;

    [SerializeField]
    private float current;

    public float Current => current;
    public float Max => maxStamina;
    public float Normalized => maxStamina <= 0f ? 0f : Mathf.Clamp01(Current / maxStamina);

    private float lastDrainTime;

    private void Awake()
    {
        current = maxStamina;
    }

    private void Update()
    {
        // regen after a short delay since last drain
        if (Time.time < lastDrainTime + regenDelayAfterDrain)
        {
            return;
        }

        if (current < maxStamina)
        {
            current = Mathf.Min(maxStamina, current + regenPerSecond * Time.deltaTime);
        }
    }

    public bool CanSprint()
    {
        return Current > 0.01f;
    }

    public void DrainSprint(float deltaTime)
    {
        if (current <= 0f)
        {
            current = 0f;
            return;
        }

        lastDrainTime = Time.time;
        current = Mathf.Max(0f, current - sprintDrainPerSecond * deltaTime);
    }
}