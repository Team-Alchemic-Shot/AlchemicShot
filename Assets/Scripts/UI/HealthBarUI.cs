using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Slider slider;

    private void Update()
    {
        if (health == null || slider == null)
        {
            return;
        }

        slider.value = health.CurrentHealth / health.MaxHealth;
    }
}