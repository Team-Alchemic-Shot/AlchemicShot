using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    public GameObject CurrentGunObj => CurrentGun.gameObject;

    public List<Gun> guns = new();

    public bool SwitchToNewGunOnPickup = true;

    public event Action<Gun> OnGunChanged;
    public event Action OnGunListChanged;

    // actions
    private readonly InputAction[] nums = new InputAction[9];
    // private readonly InputAction switchUp;
    // private readonly InputAction switchDown; 

    private void Awake()
    {
        for (int i = 0; i < nums.Length; i++)
        {
            var bind = i + 1; // map weapon 0 -> 1, etc
            nums[i] = new InputAction(name: bind.ToString(), type: InputActionType.Button);
            nums[i].AddBinding($"<Keyboard>/#({bind})");
        }
    }

    private void OnEnable()
    {
        for (int i = 0; i < nums.Length; i++)
        {
            nums[i]?.Enable();
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < nums.Length; i++)
        {
            nums[i]?.Disable();
        }
    }

    private void Start()
    {
        if (_currentGun == null && guns.Count > 0)
        {
            CurrentGun = guns[0]; // we can safely assign the revolver here
        }
        else
        {
            OnGunChanged?.Invoke(_currentGun);
        }
    }

    void Update()
    {
        for (int i = 0; i < nums.Length; i++)
        {
            if (nums[i].WasPressedThisFrame())
            {
                SwitchGun(i);
                break;
            }
        }
    }

    public void AddGun(GameObject gunPrefab)
    {
        var gunObj = Instantiate(gunPrefab, transform);
        if (gunObj.TryGetComponent<Gun>(out var gun))
        {
            guns.Add(gun);
            OnGunListChanged?.Invoke();
            if (CurrentGun == null)
            {
                CurrentGun = gun;
            }
            if (SwitchToNewGunOnPickup)
            {
                SwitchGun(guns.Count - 1); // switch to the newly added gun
            }
        }
        else
        {
            Debug.LogWarning("The provided prefab does not contain a Gun component.");
        }
    }

    public void SwitchGun(int index)
    {
        if (index >= 0 && index < guns.Count)
        {
            CurrentGunObj.SetActive(false);
            CurrentGun = guns[index];
            CurrentGunObj.SetActive(true);
            OnGunChanged?.Invoke(CurrentGun);
        }
    }
}