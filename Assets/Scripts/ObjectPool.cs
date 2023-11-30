using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private PoolableObject prefab;
    private List<PoolableObject> availableObjects;

    private ObjectPool(PoolableObject prefab, int size)
    {
        this.prefab = prefab;
        availableObjects = new List<PoolableObject>(size);
    }

    public static ObjectPool CreateInstance(PoolableObject prefab, int size)
    {
        ObjectPool pool = new ObjectPool(prefab, size);
        GameObject poolObject = new GameObject(prefab.name + "Pool");
        pool.CreateObjects(poolObject.transform, size);

        return pool;
    }

    private void CreateObjects(Transform parent, int size)
    {
        for (int i = 0; i < size; i++)
        {
            PoolableObject poolableObject = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, parent.transform);
            poolableObject.Parent = this;
            poolableObject.gameObject.SetActive(false);
        }
    }

    public void ReturnObjectToPool(PoolableObject poolableObject)
    {
        availableObjects.Add(poolableObject);
    }


    public PoolableObject GetObject()
    {
        if (availableObjects.Count > 0)
        {
            PoolableObject instance = availableObjects[0];
            availableObjects.RemoveAt(0);
            instance.gameObject.SetActive(true);
            return instance;
        }
        else
        {
            return null;
        }
    }

}
