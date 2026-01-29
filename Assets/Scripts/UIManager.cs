using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private float HPtoDisplay; 
    private TMP_Text healthText;
    private List<TMP_Text> bulletText;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // update health
        float HPtoDisplay = GameObject.FindWithTag("Player").GetComponent<Health>().CurrentHealth;
        healthText.text = HPtoDisplay.ToString();

        // update ammo
        foreach (var bullet in bulletText) {
            
        }
    }

    public void AddBullet()
    {

    }

    public void RemoveBullet()
    {
        bulletText.RemoveAt(0);
    }
}
