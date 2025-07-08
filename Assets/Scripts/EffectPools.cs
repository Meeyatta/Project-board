using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectPools : MonoBehaviour
{
    public static EffectPools Instance;
    [System.Serializable]
    public class Pool
    {
        public string Tag;
        public GameObject Prefab;
        public int Size;
        public Transform Parent;
    }
    public List<Pool> Pools = new List<Pool>();
    public Dictionary<string, Queue<GameObject>> CurrentPools = new Dictionary<string, Queue<GameObject>>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        foreach (Pool p in Pools)
        {
            Queue<GameObject> objPool = new Queue<GameObject>();
            for (int i = 0; i < p.Size; i++)
            {
                GameObject obj = Instantiate(p.Prefab, p.Parent);
                obj.SetActive(false);
                objPool.Enqueue(obj);
            }
            CurrentPools.Add(p.Tag, objPool);
        }
    }

    public GameObject InstantiateFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!CurrentPools.ContainsKey(tag)) { Debug.LogWarning("No tag in pools named " + tag); return null; }

        GameObject obj = CurrentPools[tag].Dequeue();

        obj.SetActive(true);
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        CurrentPools[tag].Enqueue(obj);

        return obj;
    }
    public void DestroyToPool(GameObject obj)
    {
        obj.SetActive(false);
    }
}
