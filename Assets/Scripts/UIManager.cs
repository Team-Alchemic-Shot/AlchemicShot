using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEditor.Progress;

public class UIManager : MonoBehaviour
{
    private float HPtoDisplay;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TextMeshProUGUI bulletTextPrefab;
    private List<TMP_Text> bulletText;
    [SerializeField] private Transform bulletPanel;
    private Gun gun;

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


        // update ammo display
        foreach (Transform text in bulletPanel)
        {
            Destroy(text.gameObject);
        }

        Stack<BulletData> bullets = gun.GetMagazine().GetBullets();
        foreach (var bullet in bullets)
        {
            TextMeshProUGUI text = Instantiate(bulletTextPrefab, bulletPanel);
            text.text = bullet.element.elementName + " Bullet";
            text.color = bullet.element.elementColor;
        }


    }

    public void AddBullet()
    {

    }

    public void RemoveBullet()
    {
        //bulletText.RemoveAt(0);
    }
}
