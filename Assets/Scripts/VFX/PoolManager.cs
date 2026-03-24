using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : SingletonMonobehaviour<PoolManager>
{
    private Dictionary<int, Queue<GameObject>> poolDictionary = new Dictionary<int, Queue<GameObject>>();
    [SerializeField] private Pool[] pool = null;
    [SerializeField] private Transform objectPoolTransform = null;

    [System.Serializable]
    public struct Pool
    {
        public int poolSize;
        public GameObject prefab;
    }

    private void Start()
    {
        for (int i = 0; i < pool.Length; ++i)
        {
            CreatePool(pool[i].prefab, pool[i].poolSize);
        }
    }

    private void CreatePool(GameObject prefab, int size)
    {
        int poolKey = prefab.GetInstanceID();
        string prefabName = prefab.name;

        GameObject parentGameObject = new GameObject(prefabName + "Anchor");
        parentGameObject.transform.SetParent(objectPoolTransform);

        if (!poolDictionary.ContainsKey(poolKey))
        {
            poolDictionary.Add(poolKey, new Queue<GameObject>());
            // Create the initial pool of objects
            for (int i = 0; i < size; ++i)
            {
                GameObject obj = Instantiate(prefab, parentGameObject.transform) as GameObject;
                obj.SetActive(false);
                poolDictionary[poolKey].Enqueue(obj);
            }
        }
    }

    public GameObject ReuseObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        int poolKey = prefab.GetInstanceID();

        if (poolDictionary.ContainsKey(poolKey))
        {
            GameObject obj = GetObjectFromPool(poolKey);
            ResetObject(position, rotation, obj, prefab);
            return obj;
        }
        else
        {
            Debug.Log("没有可用的" + prefab + "对象");
            return null;
        }
    }

    private GameObject GetObjectFromPool(int poolKey)
    {
        GameObject gameObject = poolDictionary[poolKey].Dequeue();
        poolDictionary[poolKey].Enqueue(gameObject);

        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }

        return gameObject;
    }

    private static void ResetObject(Vector3 position, Quaternion rotation, GameObject obj, GameObject prefab)
    {
        obj.transform.position = position;
        obj.transform.rotation = rotation;


        obj.transform.localScale = prefab.transform.localScale;
    }
}
