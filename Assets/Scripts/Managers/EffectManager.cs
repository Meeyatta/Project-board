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
    Coroutine CurShowingPossibleUnitPosition = null;
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
    #region Showing unit plaement when creating a new unit
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
            yield return new WaitForSeconds(0.01f);
            foreach (var v in PlacementEffectsToHide)
            {
                if (BoardManager.Instance.ClosestUnitPosToCursor(v.Key) == null) { continue; }

                List<Vector2Int> poss = BoardManager.Instance.ClosestUnitPosToCursor(v.Key);
                for (int i = 0; i < poss.Count; i++)
                {
                    //Debug.Log("Showing placement pos " + i + " " + poss[i]);
                    List<Vector2Int> sTl = new List<Vector2Int>(); sTl.Add(poss[i]);
                    v.Value[i].transform.position = BoardManager.Instance.BoardToWorldPosition(sTl).Value;

                }
                yield return new WaitForSeconds(0.01f);
            }

        }
        yield return new WaitForSeconds(0.01f);
    }
    #endregion

    #region Showing position of unit under the cursor when moving
    void StartShowingPossibleUnitPosition(Unit unit)
    {
        if (CurShowingPossibleUnitPosition == null) { CurShowingPossibleUnitPosition = StartCoroutine(ShowingPossibleUnitPosition(unit)); }
    }
    void StopShowingPossibleUnitPosition()
    {
        StopCoroutine(CurShowingPossibleUnitPosition);
        CurShowingPossibleUnitPosition = null;
    }
    IEnumerator ShowingPossibleUnitPosition(Unit unit) 
    {
        yield return new WaitForSeconds(0.1f);
        while (CurShowingPossibleUnitPosition != null)
        {
            yield return new WaitForSeconds(0.1f);
            List<Vector2Int> poss = new List<Vector2Int>();
            foreach (var v in unit.Size.Positions)
            {
                poss.Add(v + BoardManager.Instance.CursorToCellPosition());
            }
            unit.gameObject.transform.position = BoardManager.Instance.BoardToWorldPosition(poss).Value + unit.ModelOffset;

        }
        yield return new WaitForSeconds(0.1f);
    }
    #endregion

    #region Showing possible movement positions of unit
    void ShowMovement(Unit unit)
    {
      StartShowingPossibleUnitPosition(unit);

      List<GameObject> ePu = new List<GameObject>();
            #region If it's a single cell sized unit
            if (unit.Size.Positions.Count == 1)
            {
                foreach (var vv in Action_Move.Get_PossibleMovement(unit))
                {
                    foreach (var v in vv)
                    {
                        List<Vector2Int> single = new List<Vector2Int>(); single.Add(v);
                        GameObject overlay = InstantiateFromPool(Tag.Movement, BoardManager.Instance.BoardToWorldPosition(single).Value, Quaternion.identity);
                        ePu.Add(overlay);
                    }
                    if (!UnitEffectsToHide.ContainsKey(unit)) { UnitEffectsToHide.Add(unit, ePu); }
                }
            }
            #endregion
            #region else - unit is multicell
            if (unit.Size.Positions.Count > 1)
            {
                foreach (var line in Action_Move.Get_PossibleMovement_Multi(unit))
                {
                    foreach (var positions in line)
                    {
                        foreach (var v in positions)
                        {
                            List<Vector2Int> single = new List<Vector2Int>(); single.Add(v);
                            GameObject overlay = InstantiateFromPool(Tag.Movement, BoardManager.Instance.BoardToWorldPosition(single).Value, Quaternion.identity);
                            ePu.Add(overlay);
                        }
                        if (!UnitEffectsToHide.ContainsKey(unit)) { UnitEffectsToHide.Add(unit, ePu); }
                    }
                }
            }
            #endregion     
    }
    void HideMovement(Unit unit)
    {
        if (!UnitEffectsToHide.ContainsKey(unit)) return;

        StopShowingPossibleUnitPosition();

        foreach (var ef in UnitEffectsToHide[unit])
        {
             DestroyToPool(ef);
        }
       UnitEffectsToHide.Remove(unit);     
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
