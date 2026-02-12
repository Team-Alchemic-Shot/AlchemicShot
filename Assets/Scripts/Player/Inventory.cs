using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private Gun _currentGun;
    public Gun CurrentGun
    {
        get => _currentGun;
        set
        {
            if (_currentGun != value)
            {
                _currentGun = value;
                OnGunChanged?.Invoke(_currentGun);
            }
        }
    }

    public List<Gun> guns = new();

    public event Action<Gun> OnGunChanged;

    private void Start()
    {
        if (_currentGun == null && guns.Count > 0)
        {
            CurrentGun = guns[0];
        }
        else
        {
            OnGunChanged?.Invoke(_currentGun);
        }
    }

    void Update()
    {
        // switch guns check and invoke event
    }

}