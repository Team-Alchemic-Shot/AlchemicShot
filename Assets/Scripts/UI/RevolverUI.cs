using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RevolverUI : MagazineUI
{
    [SerializeField] private Transform bulletPanel;
    [SerializeField] private TextMeshProUGUI bulletTextPrefab;
    private List<BulletData> bullets = new List<BulletData>();
    private Gun gun;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void Reload()
    {
        gun = GameObject.FindWithTag("Player").GetComponentInChildren<Gun>();
        foreach (Transform text in bulletPanel)
        {
            GameObject.Destroy(text.gameObject);
        }
        foreach (BulletData bullet in gun.GetMagazine().GetBullets())
        {
            bullets.Add(bullet);
        }
    }

    public override void Shoot()
    {
        bullets.RemoveAt(bullets.Count - 1);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var bullet in bullets)
        {
            TextMeshProUGUI text = GameObject.Instantiate(bulletTextPrefab, bulletPanel);
            text.text = bullet.element.elementName + " Bullet";
            text.color = bullet.element.elementColor;
        }
    }
}
