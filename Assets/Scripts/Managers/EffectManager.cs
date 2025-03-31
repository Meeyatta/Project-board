using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;
using System.Linq;



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
    public Material ObjMat_Neutral;
    public Material ObjMat_Player;
    public Material ObjMat_Enemy;

    public GameObject CellOverlay;
    public float ModelOffset;

    public GameObject EffectsObj; //Parent of all effect objects

    #region Storing current effects
    public Dictionary<Unit, List<GameObject>> UnitEffectsToHide = new Dictionary<Unit, List<GameObject>>();
    public Dictionary<Unit, List<GameObject>> PlacementEffectsToHide = new Dictionary<Unit, List<GameObject>>();
    #endregion

    #region For creating pools of objects we create
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
    #endregion

    //Placement specific
    Coroutine CurPlacement;
    Coroutine CurUnitMovePosShowcase;

    const string isRaisedStr = "isRaised";


    void Start()
    {
        EffectsObj = GameObject.Find("Effects");
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

        #region EventsAdd
        GameManager.Instance.ShowMovementEvent.AddListener(ShowMovement);
        GameManager.Instance.HideMovementEvent.AddListener(HideMovement);

        GameManager.Instance.ShowDeploymentEvent.AddListener(ShowDeployment);
        GameManager.Instance.HideDeploymentEvent.AddListener(HideDeployment);

        GameplayManager.Instance.eBattlefieldCreation.AddListener(StartUpdatingObjectiveControl);

        ScoreManager.Instance.EndPlayerTurnEvent.AddListener(HideMovement);

        GameManager.Instance.ShowPlacementEvent.AddListener(StartShowingPlacement);
        GameManager.Instance.HidePlacementEvent.AddListener(StopShowingPlacement);
        #endregion EventsAdd
    }
    #region EventsRemove
    void OnDisable()
    {
        GameManager.Instance.ShowMovementEvent.RemoveListener(ShowMovement);
        GameManager.Instance.HideMovementEvent.RemoveListener(HideMovement);

        GameManager.Instance.ShowDeploymentEvent.RemoveListener(ShowDeployment);
        GameManager.Instance.HideDeploymentEvent.RemoveListener(HideDeployment);

        GameplayManager.Instance.eBattlefieldCreation.RemoveListener(StartUpdatingObjectiveControl);

        ScoreManager.Instance.EndPlayerTurnEvent.RemoveListener(HideMovement);

        GameManager.Instance.ShowPlacementEvent.RemoveListener(StartShowingPlacement);
        GameManager.Instance.HidePlacementEvent.RemoveListener(StopShowingPlacement);
    }
    #endregion EventsRemove

    #region Singleton
    public static EffectManager Instance;
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

    #region Visually updating objectives depending on who controls them
    public void StartUpdatingObjectiveControl()
    {
        StartCoroutine(UpdatingObjectiveControl()); 
    }

    IEnumerator UpdatingObjectiveControl()
    {
        yield return new WaitForSeconds(Time.deltaTime * 40);

        Dictionary<Unit, MeshRenderer> list = new Dictionary<Unit, MeshRenderer>(); 
        foreach (var v in BoardManager.Instance.Get_AllUnitsWithKeywords( new List<Keyword> { Keyword.Objective }))
        {
            //Debug.Log(v.name + " " +
            //    v.transform.Find("ModelHolder").name +
            //    v.transform.Find("ModelHolder").transform.Find("Objective").name);

            list.Add(v, v.transform.Find("ModelHolder").transform.Find("Objective").GetComponent<MeshRenderer>());
        }

        while (true)
        {
            foreach (var v in list)
            {
                int s = Ability_Score.Check(v.Key);

                v.Value.material = ObjMat_Neutral;
                if (s > 0) { v.Value.material = ObjMat_Player; }
                if (s < 0) { v.Value.material = ObjMat_Enemy; }

            }

            yield return new WaitForSeconds(Time.deltaTime);
        }
        
    }
    #endregion

    #region For raising and lowering unit's model
    void RaiseModel(Unit u)
    {
        Animator anim = u.Anim;

        anim.SetBool(isRaisedStr, true);          
    }

    void LowerModel(Unit u)
    {
        Animator anim = u.Anim;

        anim.SetBool(isRaisedStr, false);
        
    }
    #endregion

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
        yield return new WaitForSeconds(Time.deltaTime * 0.05f);
        while (CurPlacement != null && ShowingPlacementConditions())
        {
            yield return new WaitForSeconds(Time.deltaTime * 0.05f);
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
    void StartShowingPossibleUnitMovement(Unit unit)
    {
        if (CurUnitMovePosShowcase == null) 
        {
            CurUnitMovePosShowcase = StartCoroutine(ShowingPossibleUnitMovement(unit)); 
        }

    }
    void StopShowingPossibleUnitMovement(Unit unit)
    {

        StopCoroutine(ShowingPossibleUnitMovement(unit)); 
        unit.UnitModelShowcase.SetActive(false);
        CurUnitMovePosShowcase = null;
    }
    IEnumerator ShowingPossibleUnitMovement(Unit unit)
    {
        yield return new WaitForSeconds(Time.deltaTime * 0.01f);

        RaiseModel(unit);

        while (CurUnitMovePosShowcase != null && ScoreManager.Instance.PlayerTurnActionCondition())
        {
            yield return new WaitForSeconds(Time.deltaTime * 0.001f);
            if (BoardManager.Instance.ClosestUnitPosToCursor(unit) == null) { continue; }

            List<Vector2Int> poss = BoardManager.Instance.ClosestUnitPosToCursor(unit);
            for (int i = 0; i < poss.Count; i++)
            {
                List<Vector2Int> l = new List<Vector2Int> { BoardManager.Instance.CursorToCellPosition() };
                List<Vector2Int> ll = Action_Move.Get_MovementPositionsFromSingleCoordinate(unit, l);

                if (BoardManager.Instance.BoardToWorldPosition(ll).HasValue)
                {
                    unit.UnitModelShowcase.SetActive(true);
                    unit.UnitModelShowcase.transform.position = BoardManager.Instance.BoardToWorldPosition(ll).Value + unit.ModelOffset;
                }              
            }

        }

        LowerModel(unit);
        CurUnitMovePosShowcase = null;
        unit.UnitModelShowcase.SetActive(false);

    }
    #endregion

    #region Showing position of unit under the cursor when redeploying
    void StartShowingPossibleUnitRedeployment(Unit unit)
    {
        if (CurUnitMovePosShowcase == null)
        {
            CurUnitMovePosShowcase = StartCoroutine(ShowingPossibleUnitRedeployment(unit));
        }

    }
    void StopShowingPossibleUnitRedeployment(Unit unit)
    {

        StopCoroutine(ShowingPossibleUnitRedeployment(unit));
        unit.UnitModelShowcase.SetActive(false);
        CurUnitMovePosShowcase = null;
    }
    IEnumerator ShowingPossibleUnitRedeployment(Unit unit)
    {
        yield return new WaitForSeconds(Time.deltaTime * 0.01f);

        RaiseModel(unit);

        while (CurUnitMovePosShowcase != null && ScoreManager.Instance.PlayerTurnActionCondition())
        {
            yield return new WaitForSeconds(Time.deltaTime * 0.001f);
            if (BoardManager.Instance.ClosestUnitPosToCursor(unit) == null) { continue; }

            List<Vector2Int> poss = BoardManager.Instance.ClosestUnitPosToCursor(unit);
            for (int i = 0; i < poss.Count; i++)
            {
                List<Vector2Int> l = new List<Vector2Int> { BoardManager.Instance.CursorToCellPosition() };
               
                List<List<Vector2Int>> ll = Action_Redeploy.Get_PossibleDeployments(unit);

                if (ll.Any(p => p.SequenceEqual(l)))
                {
                    unit.UnitModelShowcase.SetActive(true);
                    unit.UnitModelShowcase.transform.position = BoardManager.Instance.BoardToWorldPosition(l).Value + unit.ModelOffset;
                }
                else
                {

                }
            }

        }

        CurUnitMovePosShowcase = null;
        LowerModel(unit);
        unit.UnitModelShowcase.SetActive(false);

    }
    #endregion

    #region Showing possible movement positions of unit
    void ShowMovement(List<Unit> units)
    {
        foreach (Unit u in units)
        {
            List<GameObject> ePu = new List<GameObject>();
            StartShowingPossibleUnitMovement(u);
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
            StopShowingPossibleUnitMovement(u);

            foreach (var ef in UnitEffectsToHide[u])
            {
                DestroyToPool(ef);
            }
            UnitEffectsToHide.Remove(u);
        }
    }
    #endregion

    #region Showing possible deployment positions 
    void ShowDeployment(List<Unit> units)
    {
        foreach (Unit u in units)
        {
            List<GameObject> ePu = new List<GameObject>();
            StartShowingPossibleUnitRedeployment(u);

            #region Going through all deployment zone positions remaining
            List<Vector2Int> zone = new List<Vector2Int>();
            if (u.CurKeywords.Contains(Keyword.Player)) { zone.AddRange(BoardManager.Instance.PlayerDeploymentZone); }
            for (int x = zone[0].x; x <= zone[1].x; x++)
            {
                for (int y = zone[0].y; y <= zone[1].y; y++)
                {
                    List<Vector2Int> positions = new List<Vector2Int>(); positions.AddRange(u.Size.Positions);
                    for (int i = 0; i < positions.Count; i++) { positions[i] += new Vector2Int(x, y); }

                    bool isApplicable = true;
                    #region Check if all positions are within the deployment zone
                    foreach (var v in positions)
                    {
                        if (v.x < zone[0].x || v.x > zone[1].x || v.y < zone[0].y || v.y > zone[1].y) { isApplicable = false; break; }
                        if (BoardManager.Instance.Board[x].Cells[y].CurUnit != null) { isApplicable = false; break; }
                    }
                    #endregion

                    if (isApplicable) 
                    {
                        GameObject overlay = InstantiateFromPool(Tag.Movement, BoardManager.Instance.BoardToWorldPosition(positions).Value, 
                            Quaternion.identity);
                        ePu.Add(overlay);
                    }
                }
            }
            #endregion

            if (!UnitEffectsToHide.ContainsKey(u)) { UnitEffectsToHide.Add(u, ePu); }
            
        }
    }
    void HideDeployment(List<Unit> units)
    {
        //Debug.Log("Hid movement");
        foreach (Unit u in units)
        {
            if (!UnitEffectsToHide.ContainsKey(u)) return;
            StopShowingPossibleUnitRedeployment(u);

            foreach (var ef in UnitEffectsToHide[u])
            {
                DestroyToPool(ef);
            }
            UnitEffectsToHide.Remove(u);
        }
    }
    #endregion

    #region Working with object pools
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
    #endregion

}
