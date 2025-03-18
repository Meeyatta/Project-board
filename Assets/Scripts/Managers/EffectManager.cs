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
    public float ModelOffset;
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
    Coroutine CurUnitMovePosShowcase;

     
    void Start()
    {
        EffectsObj = GameObject.Find("Effects");

        #region EventsAdd
        GameManager.Instance.ShowMovementEvent.AddListener(ShowMovement);
        GameManager.Instance.HideMovementEvent.AddListener(HideMovement);
        ScoreManager.Instance.EndPlayerTurnEvent.AddListener(HideMovement);

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
    #region Showing unit placement when creating a new unit
    void StartShowingPlacement(List<Unit> units)
    {
        if (CurPlacement == null)
        {
            foreach (Unit u in units)
            {

                List<Vector2Int> v2 = new List<Vector2Int>();
                v2 = BoardManager.Instance.ClosestUnitPosToCursor(u);

                //Works if the cursor is inside a board
                if (v2 != null && v2.Count > 0)
                {
                    //Safeguard in case the position is actually null
                    Vector3 curPos = new Vector3(-90, -90, -90);
                    curPos = BoardManager.Instance.BoardToWorldPosition(v2).Value;

                    List<GameObject> list = new List<GameObject>();
                    foreach (var v in v2)
                    {
                        list.Add(InstantiateFromPool(Tag.Placement, curPos, Quaternion.identity));
                    }

                    if (!PlacementEffectsToHide.ContainsKey(u)) { PlacementEffectsToHide.Add(u, list); }
                }
                //If the cursor is outside of the board, spawn them, but do it really far away
                else
                {
                    List<GameObject> list = new List<GameObject>();
                    for (int i = 0; i < u.Size.Positions.Count; i++)
                    {
                        list.Add(InstantiateFromPool(Tag.Placement, new Vector3(-99,-99,-99), Quaternion.identity));
                    }
                    if (!PlacementEffectsToHide.ContainsKey(u)) { PlacementEffectsToHide.Add(u, list); }
                }
                
            }

            CurPlacement = StartCoroutine(ShowingPlacement(units));
        }
    }
    
    void StopShowingPlacement(List<Unit> units)
    {
        
        foreach (var v in PlacementEffectsToHide) 
        {
            if (units.Contains(v.Key)) { foreach (var vv in v.Value) { v.Key.UnitModelShowcase.SetActive(false); DestroyToPool(vv); } }
        }
        PlacementEffectsToHide.Clear();

        if (CurPlacement != null) StopCoroutine(CurPlacement);
        CurPlacement = null;
    }

    bool ShowingPlacementConditions()
    {
        if (ScoreManager.Instance.CurTurn != ScoreManager.Side.Player) return false;

        return true;
    }
    IEnumerator ShowingPlacement(List<Unit> units)
    {
        yield return new WaitForSeconds(Time.deltaTime);
        while (CurPlacement != null && ShowingPlacementConditions())
        {
            yield return new WaitForSeconds(Time.deltaTime);
            foreach (var v in PlacementEffectsToHide)
            {
                if (BoardManager.Instance.ClosestUnitPosToCursor(v.Key) == null) { continue; }

                List<Vector2Int> poss = BoardManager.Instance.ClosestUnitPosToCursor(v.Key);
                v.Key.UnitModelShowcase.SetActive(true);
                v.Key.UnitModelShowcase.transform.position = BoardManager.Instance.BoardToWorldPosition(poss).Value + v.Key.ModelOffset;
                for (int i = 0; i < poss.Count; i++)
                {
                    //Debug.Log("Showing placement pos " + i + " " + poss[i]);
                    List<Vector2Int> sTl = new List<Vector2Int>(); sTl.Add(poss[i]);
                    
                    v.Value[i].transform.position = BoardManager.Instance.BoardToWorldPosition(sTl).Value;

                }
            }

        }

        CurPlacement = null;
    }
    #endregion

    #region Showing position of unit under the cursor when moving
    void StartShowingPossibleUnitPosition(Unit unit)
    {
        if (CurUnitMovePosShowcase == null) 
        {
            CurUnitMovePosShowcase = StartCoroutine(ShowingPossibleUnitPosition(unit)); 
        }

    }
    void StopShowingPossibleUnitPosition(Unit unit)
    {

        StopCoroutine(ShowingPossibleUnitPosition(unit)); 
        unit.UnitModelShowcase.SetActive(false);
        CurUnitMovePosShowcase = null;
    }
    IEnumerator ShowingPossibleUnitPosition(Unit unit) 
    {
        yield return new WaitForSeconds(Time.deltaTime);

        Vector3 ogPos = unit.transform.position;            //Change this to a unique raising animation since it can result 
        unit.transform.position += Vector3.up * ModelOffset;//in some problems with units model not moving with the unit

        while (CurUnitMovePosShowcase != null && ScoreManager.Instance.PlayerTurnActionCondition()) 
        {
            yield return new WaitForSeconds(Time.deltaTime);
            if (BoardManager.Instance.ClosestUnitPosToCursor(unit) == null) { continue; }

            List<Vector2Int> poss = BoardManager.Instance.ClosestUnitPosToCursor(unit);
            for (int i = 0; i < poss.Count; i++)
            {
                List<Vector2Int> l = new List<Vector2Int> { BoardManager.Instance.CursorToCellPosition() };
                List<Vector2Int> ll = Action_Move.Get_PositionsFromSingleCoordinate(unit, l);

                if (BoardManager.Instance.BoardToWorldPosition(ll).HasValue)
                {
                    unit.UnitModelShowcase.SetActive(true);
                    unit.UnitModelShowcase.transform.position = BoardManager.Instance.BoardToWorldPosition(ll).Value + unit.ModelOffset;
                }              
            }

        }

        CurUnitMovePosShowcase = null;
        unit.transform.position = ogPos; //This should also be changed to work with the animator
        unit.UnitModelShowcase.SetActive(false);

    }
    #endregion

    #region Showing possible movement positions of unit
    void ShowMovement(List<Unit> units)
    {
        foreach (Unit u in units)
        {
            List<GameObject> ePu = new List<GameObject>();
            StartShowingPossibleUnitPosition(u);
                foreach (var v in Action_Move.Get_PossibleMovement(u))
                {
                    foreach (var vv in v)
                    {
                        foreach (var vvv in vv)
                        {
                            List<Vector2Int> single = new List<Vector2Int>(); single.Add(vvv);
                            GameObject overlay = InstantiateFromPool(Tag.Movement, BoardManager.Instance.BoardToWorldPosition(single).Value, Quaternion.identity);
                            ePu.Add(overlay);
                        }                        
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
            StopShowingPossibleUnitPosition(u);

            foreach (var ef in UnitEffectsToHide[u])
            {
                DestroyToPool(ef);
            }
            UnitEffectsToHide.Remove(u);
        }
    }
    #endregion

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
