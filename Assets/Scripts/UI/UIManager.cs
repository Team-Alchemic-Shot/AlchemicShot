using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private float HPtoDisplay;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text scoreText;
    
    private Gun gun;
    private MagazineUI magazineUI;

    // Start is called before the first frame update
    void Start()
    {
        gun = GameObject.FindWithTag("Player").GetComponentInChildren<Gun>();
    }

    // Update is called once per frame
    void Update()
    {
        // update health
        float HPtoDisplay = GameObject.FindWithTag("Player").GetComponent<Health>().CurrentHealth;
        healthText.text = HPtoDisplay.ToString();
        // TODO: update score

    }
}
