using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


//Handles visual effects in the game

/*
  Functionality:
    ShowMovement(List<Unit> units) - Recieves a certain number of units, shows their possible movement by pulling overlay objects from a pool
    HideMovement(List<Unit> units) - Receives a certain number of units, hides their movement objects back into the pool, tracks them in UnitEffectsToHide
    InstantiateFromPool(string tag, Vector3 position, Quaternion rotation) - Enables an object from an existing pool
    DestroyToPool(GameObject obj) - Disables an object from an existing pool

*/

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;

    public GameObject CellOverlay;
    public Dictionary<Unit, List<GameObject>> UnitEffectsToHide = new Dictionary<Unit, List<GameObject>>();

    public GameObject EffectsObj;

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

     
    void Start()
    {
        EffectsObj = GameObject.Find("Effects");

        #region EventsAdd
        GameManager.Instance.ShowMovementEvent.AddListener(ShowMovement);
        GameManager.Instance.HideMovementEvent.AddListener(HideMovement);
        #endregion EventsAdd
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
    #region EventsRemove
    void OnDisable()
    {
        GameManager.Instance.ShowMovementEvent.RemoveListener(ShowMovement);
        GameManager.Instance.HideMovementEvent.RemoveListener(HideMovement);
    }
    #endregion EventsRemove
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

    void ShowMovement(List<Unit> units)
    {
        foreach (Unit u in units)
        {
            List<GameObject> ePu = new List<GameObject>();
            foreach (var vv in GameManager.Instance.GetPossibleMovement(u))
            {
                foreach (var v in vv)
                {
                    List<Vector2Int> single = new List<Vector2Int>(); single.Add(v);
                    GameObject overlay = InstantiateFromPool("Movement", BoardManager.Instance.BoardToWorldPosition(single).Value, Quaternion.identity);
                    ePu.Add(overlay);
                }                         
            }
            if (!UnitEffectsToHide.ContainsKey(u)) { UnitEffectsToHide.Add(u, ePu); }         
        }
    }
    void HideMovement(List<Unit> units)
    {
        //Debug.Log("Hid movement");
        foreach (Unit u in units)
        {
            if (!UnitEffectsToHide.ContainsKey(u)) return;

            foreach(var ef in UnitEffectsToHide[u])
            {
                DestroyToPool(ef);
            }
            UnitEffectsToHide.Remove(u);
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
