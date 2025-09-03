using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager_ObjPools : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string Name;
        public int Size;
        public GameObject Prefab;
        public Transform Parent;
    }

    public List<Pool> Pools = new List<Pool>();
    public Dictionary<string, Queue<GameObject>> Cur_Pools = new Dictionary<string, Queue<GameObject>>();

    #region Singleton
    public static BoardManager_ObjPools Instance;
    void Singleton()
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
    private void Awake()
    {
        Singleton();
    }
    #endregion

    // Start is called before the first frame update
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
            Cur_Pools.Add(p.Name, objPool);
        }

        Debug.Log("Created " + Cur_Pools["cell"].Count + " cells");
    }

    #region Working with object pools
    public GameObject InstantiateFromPool(string name, Vector3 position, Quaternion rotation)
    {
        if (!Cur_Pools.ContainsKey(name)) { Debug.LogWarning("No name in pools named " + tag); return null; }

        GameObject obj = Cur_Pools[name].Dequeue();

        obj.SetActive(true);
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        Cur_Pools[name].Enqueue(obj);

        return obj;
    }
    public void DestroyToPool(GameObject obj)
    {
        obj.SetActive(false);
    }
    #endregion

    // Update is called once per frame
    void Update()
    {
        
    }
}
