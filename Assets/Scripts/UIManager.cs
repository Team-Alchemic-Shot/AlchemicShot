using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    float HPtoDisplay; 
    public TMP_Text healthText;

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

    }

    public void AddBullet()
    {

    }

    public void RemoveBullet()
    {

    }
}
