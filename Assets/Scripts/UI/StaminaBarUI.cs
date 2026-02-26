using UnityEngine;
using UnityEngine.UI;

public class StaminaBarUI : MonoBehaviour
{
    [SerializeField] private Stamina stamina;
    [SerializeField] private Slider slider;

    private void Update()
    {
        if (stamina == null || slider == null)
        {
            return;
        }

        slider.value = stamina.Normalized;
    }
}