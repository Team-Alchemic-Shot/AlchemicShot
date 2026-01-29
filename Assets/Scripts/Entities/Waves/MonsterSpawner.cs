using System;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public event Action<GameObject> OnMonsterSpawned;

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject instance = Instantiate(prefab, position, rotation);
        OnMonsterSpawned?.Invoke(instance);
        return instance;
    }
}
