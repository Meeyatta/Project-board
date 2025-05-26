using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;
using System.Linq;
using Unity.VisualScripting;



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
    public Dictionary<Unit, List<GameObject>> MovementEffectsToHide = new Dictionary<Unit, List<GameObject>>();
    public Dictionary<Unit, List<GameObject>> DeploymentEffectsToHide = new Dictionary<Unit, List<GameObject>>();
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
        GameManager.Instance.E_ShowMovement.AddListener(ShowMovement);
        GameManager.Instance.E_HideMovement.AddListener(HideMovement);

        GameManager.Instance.E_ShowDeployment.AddListener(ShowDeployment);
        GameManager.Instance.E_HideDeployment.AddListener(HideDeployment);

        if (BattlefieldManager.Instance != null) BattlefieldManager.Instance.eBattlefieldCreation.AddListener(StartUpdatingObjectiveControl);

        Action_NextTurn.E_Turn_Functional.AddListener(HideAllPlayerMovement);

        GameManager.Instance.E_ShowPlacement.AddListener(StartShowingPlacement);
        GameManager.Instance.E_HidePlacement.AddListener(StopShowingPlacement);
        #endregion EventsAdd
    }

    #region EventsRemove
    void OnDisable()
    {
        GameManager.Instance.E_ShowMovement.RemoveListener(ShowMovement);
        GameManager.Instance.E_HideMovement.RemoveListener(HideMovement);

        GameManager.Instance.E_ShowDeployment.RemoveListener(ShowDeployment);
        GameManager.Instance.E_HideDeployment.RemoveListener(HideDeployment);

        if (BattlefieldManager.Instance != null) BattlefieldManager.Instance.eBattlefieldCreation.RemoveListener(StartUpdatingObjectiveControl);

        Action_NextTurn.E_Turn_Functional.RemoveListener(HideAllPlayerMovement);

        GameManager.Instance.E_ShowPlacement.RemoveListener(StartShowingPlacement);
        GameManager.Instance.E_HidePlacement.RemoveListener(StopShowingPlacement);
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

    #region Everything referring to covering board cells

    #region Referring to drawing effect area placement
    Coroutine c_DrawingEffectArea = null;
    public void Start_DrawingEffectAreaPlacement()
    {
        if (c_DrawingEffectArea == null)
        {
            c_DrawingEffectArea = StartCoroutine(DrawingEffectAreaPlacement());
        }
    }

    public IEnumerator DrawingEffectAreaPlacement()
    {
        yield return new WaitForSeconds(Time.fixedDeltaTime);
    }
    public void Stop_DrawingEffectAreaPlacement()
    {
        if (c_DrawingEffectArea != null)
        {
            StopCoroutine(c_DrawingEffectArea);
            c_DrawingEffectArea = null;
        }
    }
    #endregion

    #region Drawing covers on top of board cells

    //These may need to be remade into coroutines in case i will need specific timings instead of it being instant
    #region Starts drawing new covers of selected type on selected coordinates
    public void StartDrawingCovers(CoverType type, List<Vector2Int> coords)
    {
        StopDrawingCovers(coords);
    }
    #endregion

    #region Clears all covers on selected coordinates
    public void StopDrawingCovers(List<Vector2Int> coords)
    {
        foreach (var v in coords)
        {
            if (!BoardManager.Instance.IsInBounds(v)) { continue; }

            //Stopped here
        }
    }
    #endregion

    #endregion
    #endregion

    #region Visually updating objectives depending on who controls them
    public void StartUpdatingObjectiveControl()
    {
        StartCoroutine(UpdatingObjectiveControl());
    }

    IEnumerator UpdatingObjectiveControl()
    {
        yield return new WaitForSeconds(Time.fixedDeltaTime * 40);

        Dictionary<Unit, MeshRenderer> list = new Dictionary<Unit, MeshRenderer>();
        foreach (var v in BoardManager.Instance.Get_AllUnitsWithKeywords(new List<Keyword> { Keyword.Objective }))
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

            yield return new WaitForSeconds(Time.fixedDeltaTime);
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
        //Debug.Log("Lowered the unit");

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
                        list.Add(InstantiateFromPool(Tag.Placement, new Vector3(-99, -99, -99), Quaternion.identity));
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
        if (BattleStatsManager.Instance.CurTurn != Side.Player) return false;

        return true;
    }
    IEnumerator ShowingPlacement(List<Unit> units)
    {
        yield return new WaitForSeconds(Time.fixedDeltaTime * 0.05f);
        while (CurPlacement != null && ShowingPlacementConditions())
        {
            yield return new WaitForSeconds(Time.fixedDeltaTime * 0.05f);
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

    IEnumerator ShowingPossibleUnitPosition(Unit unit, List<Vector2Int> availableZone)
    {
        yield return new WaitForSeconds(Time.fixedDeltaTime * 0.01f);

        RaiseModel(unit);

        while (CurUnitMovePosShowcase != null && BattleStatsManager.Instance.PlayerTurnActionCondition())
        {
            yield return new WaitForSeconds(Time.fixedDeltaTime * 0.1f);
            //Debug.Log("ShowingPossibleUnitPosition");

            List<Vector2Int> pos = BoardManager.Instance.ClosestUnitPosToCursor(unit);
            if (availableZone != null && availableZone.Count > 0 && pos != null && pos.Count > 0 && 
                availableZone.Intersect<Vector2Int>(pos).Any())
            {
                unit.UnitModelShowcase.SetActive(true);
                unit.UnitModelShowcase.transform.position = BoardManager.Instance.BoardToWorldPosition(pos).Value + unit.ModelOffset;
            }
        }

        LowerModel(unit);
        CurUnitMovePosShowcase = null;
        unit.UnitModelShowcase.SetActive(false);
    }

    #region Showing position of unit under the cursor when moving
    void StartShowingPossibleUnitMovement(Unit unit)
    {
        if (CurUnitMovePosShowcase == null)
        {
            List<Vector2Int> poss = new List<Vector2Int>();
            foreach (var v in Action_Move.Get_PossibleMovement(unit)) { foreach (var vv in v) { poss.AddRange(vv); } }
            CurUnitMovePosShowcase = StartCoroutine(ShowingPossibleUnitPosition(unit, poss));
        }

    }
    void StopShowingPossibleUnitMovement(Unit unit)
    {
        LowerModel(unit);

        if (CurUnitMovePosShowcase != null) StopCoroutine(CurUnitMovePosShowcase);
        CurUnitMovePosShowcase = null;

        unit.UnitModelShowcase.SetActive(false);
    }

    #endregion

    #region Showing position of unit under the cursor when redeploying
    void StartShowingPossibleUnitRedeployment(Unit unit)
    {
        //Debug.Log("StartShowingPossibleUnitRedeployment");

        if (CurUnitMovePosShowcase == null)
        {
            List<Vector2Int> poss = new List<Vector2Int>();
            foreach (var v in Action_Redeploy.Get_PossibleDeployments(unit)) { foreach (var vv in v) { poss.Add(vv); } }

            CurUnitMovePosShowcase = StartCoroutine(ShowingPossibleUnitPosition(unit, poss));
        }

    }
    #endregion

    #region Showing possible movement positions of unit
    void ShowMovement(List<Unit> units)
    {
        foreach (Unit u in units)
        {
            if (MovementEffectsToHide.ContainsKey(u)) continue;

            //Debug.Log("Called to show possible movement of " + u.gameObject.name);
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
                if (!MovementEffectsToHide.ContainsKey(u)) { MovementEffectsToHide.Add(u, ePu); }
            }
        }
    }
    public void HideAllPlayerMovement(Side s)
    {
        List<Unit> units = BoardManager.Instance.Get_AllUnitsWithKeywords(new List<Keyword> { Keyword.Player });

        HideMovement(units);
    }
    void HideMovement(List<Unit> units)
    {
        foreach (Unit u in units)
        {
            if (!MovementEffectsToHide.ContainsKey(u)) return;
            StopShowingPossibleUnitMovement(u);

            foreach (var ef in MovementEffectsToHide[u])
            {
                DestroyToPool(ef);
            }
            MovementEffectsToHide.Remove(u);
        }
    }
    #endregion

    #region Showing possible deployment positions 
    void ShowDeployment(List<Unit> units)
    {
        foreach (Unit u in units)
        {
            if (DeploymentEffectsToHide.ContainsKey(u)) continue;

            List<GameObject> ePu = new List<GameObject>();
            StartShowingPossibleUnitRedeployment(u);

            #region Going through all deployment zone positions remaining
            List<Vector2Int> zone = new List<Vector2Int>();
            if (u.CurKeywords.Contains(Keyword.Player)) { zone.AddRange(BoardManager.Instance.PlayerDeploymentZone); }
            if (zone.Count >= 2)
            {
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
            }  
            #endregion

            if (!DeploymentEffectsToHide.ContainsKey(u)) { DeploymentEffectsToHide.Add(u, ePu); }

        }
    }
    
    void HideDeployment()
    {
        Dictionary<Unit, List<GameObject>> copy = new Dictionary<Unit, List<GameObject>>();
        copy.AddRange(DeploymentEffectsToHide);

        foreach (var v in copy)
        {
            CurUnitMovePosShowcase = null;

            foreach (var vv in v.Value) { DestroyToPool(vv); DeploymentEffectsToHide.Remove(v.Key); }
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
