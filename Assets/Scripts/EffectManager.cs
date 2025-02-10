using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;


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
    public Dictionary<Unit, List<GameObject>> PlacementEffectsToHide = new Dictionary<Unit, List<GameObject>>();

    [System.Serializable]
    public class Pool
    {
        public Tag Tag;
        public GameObject Prefab;
        public int Size;
        public Transform Parent;
    }
    public enum Tag { Movement, Placement };
    public List<Pool> Pools = new List<Pool>();
    public Dictionary<Tag, Queue<GameObject>> CurrentPools = new Dictionary<Tag, Queue<GameObject>>();

    //Placement specific
    Coroutine CurPlacement;

     
    void Start()
    {
        EffectsObj = GameObject.Find("Effects");

        #region EventsAdd
        GameManager.Instance.ShowMovementEvent.AddListener(ShowMovement);
        GameManager.Instance.HideMovementEvent.AddListener(HideMovement);

        GameManager.Instance.ShowPlacementEvent.AddListener(StartShowingPlacement);
        GameManager.Instance.HidePlacementEvent.AddListener(StopShowingPlacement);
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
    void StartShowingPlacement(List<Unit> units)
    {
        if (CurPlacement == null)
        {
            foreach (Unit u in units)
            {
                List<Vector2Int> v2 = BoardManager.Instance.CursorToCellPosition();
                for (int i = 1; i < u.Size.Positions.Count; i++) { v2.Add(u.Size.Positions[i]); }

                //Safeguard in case the position is actually null
                Vector3 curPos = new Vector3(-90, -90, -90);
                if (BoardManager.Instance.CursorToCellPosition()[0].x > -50) { curPos = BoardManager.Instance.BoardToWorldPosition(v2).Value; }

                List<GameObject> list = new List<GameObject>();
                foreach (var v in v2)
                {
                    list.Add(InstantiateFromPool(Tag.Placement, curPos, Quaternion.identity));
                }
                if (!PlacementEffectsToHide.ContainsKey(u)) { PlacementEffectsToHide.Add(u, list); }
            }

            CurPlacement = StartCoroutine(ShowingPlacement(units));
        }
    }
    void StopShowingPlacement(List<Unit> units)
    {
        foreach (var v in PlacementEffectsToHide) 
        {
            if (units.Contains(v.Key)) { foreach (var vv in v.Value) { DestroyToPool(vv); } }
        }
        PlacementEffectsToHide.Clear();

        if (CurPlacement != null) StopCoroutine(CurPlacement);
        CurPlacement = null;
    }
    IEnumerator ShowingPlacement(List<Unit> units)
    {
        yield return new WaitForSeconds(0.1f);
        while (CurPlacement != null)
        {
            foreach (var v in PlacementEffectsToHide)
            {
                for (int i = 0; i < v.Key.Size.Positions.Count; i++)
                {
                    List<Vector2Int> posP = new List<Vector2Int>();
                    if (BoardManager.Instance.CursorToCellPosition().Count <= 0) { continue; }
                    posP.Add(BoardManager.Instance.CursorToCellPosition()[0] + v.Key.Size.Positions[i]);

                    //Safeguard in case the position is actually null
                    if (BoardManager.Instance.CursorToCellPosition()[0].x < -50) { v.Value[i].transform.position = new Vector3(-90, -90, -90); }
                    else { v.Value[i].transform.position = BoardManager.Instance.BoardToWorldPosition(posP).Value; }
                }
                yield return new WaitForSeconds(0.01f);
            }

        }
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
                    GameObject overlay = InstantiateFromPool(Tag.Movement, BoardManager.Instance.BoardToWorldPosition(single).Value, Quaternion.identity);
                    ePu.Add(overlay);
                }
                if (!UnitEffectsToHide.ContainsKey(u)) { UnitEffectsToHide.Add(u, ePu); }
            }       
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
    public GameObject InstantiateFromPool(Tag tag, Vector3 position, Quaternion rotation) 
    {
        if (!CurrentPools.ContainsKey(Tag.Movement)) { Debug.LogWarning("No tag in pools named " + tag); return null; }

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
