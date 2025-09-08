using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.UIElements;



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

    public float ModelOffset;

    public GameObject EffectsObj; //Parent of all effect objects

    public bool ShouldDebug;

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
    public enum Tag { Placement, Creation, Water, Oil };
    public List<Pool> Pools = new List<Pool>();
    public Dictionary<Tag, Queue<GameObject>> CurrentPools = new Dictionary<Tag, Queue<GameObject>>();
    #endregion

    //Placement specific
    Coroutine CurPlacement;

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

        if (BattleManager.Instance != null) BattleManager.Instance.eBattlefieldCreation.AddListener(StartUpdatingObjectiveControl);

        BattleStatsManager.Instance.E_Turn_Functional.AddListener(HideAllPlayerMovement);

        #endregion EventsAdd
    }

    #region EventsRemove
    void OnDisable()
    {
        GameManager.Instance.E_ShowMovement.RemoveListener(ShowMovement);
        GameManager.Instance.E_HideMovement.RemoveListener(HideMovement);

        GameManager.Instance.E_ShowDeployment.RemoveListener(ShowDeployment);
        GameManager.Instance.E_HideDeployment.RemoveListener(HideDeployment);

        if (BattleManager.Instance != null) BattleManager.Instance.eBattlefieldCreation.RemoveListener(StartUpdatingObjectiveControl);

        BattleStatsManager.Instance.E_Turn_Functional.RemoveListener(HideAllPlayerMovement);

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

    List<Vector2Int> OnlyCoveredCells()
    {
        List<Vector2Int> l = new List<Vector2Int>();
        for (int x = 0; x < BoardManager.Instance.Board.Count; x++)
        {
            for (int y = BoardManager.Instance.Board[x].Cells.Count - 1; y >= 0; y--)
            {
                if (BoardManager.Instance.Board[x].Cells[y].CoveredBy != CoverType.None)
                {
                    l.Add(new Vector2Int(x, y));
                }
            }
        }

        return l;
    }
    public bool ShouldDrawCovers = true;
    Coroutine c_DrawingCovers = null;
    public Dictionary<Vector2Int, GameObject> CoverEffectsToHide = new Dictionary<Vector2Int, GameObject>();
    IEnumerator DrawingCovers()
    {
        if (ShouldDebug) Debug.Log("Starting to draw covers");
        yield return new WaitForSeconds(Time.fixedDeltaTime);

        while (ShouldDrawCovers && c_DrawingCovers != null)
        {
            yield return new WaitForSeconds(Time.fixedDeltaTime);

            #region Cleaning cells without covers
            if (ShouldDebug) Debug.Log("Handling " + CoverEffectsToHide.Count + " cover effects");
            List<Vector2Int> toRemove = new List<Vector2Int>();
            foreach (var v in CoverEffectsToHide)
            {
                CoverType vCover = BoardManager.Instance.Board[v.Key.x].Cells[v.Key.y].CoveredBy;
                if (vCover == CoverType.None)
                {
                    if (ShouldDebug) Debug.Log("Hiding on" + v + " the " + v.Value.name);
                    DestroyToPool(v.Value);
                    toRemove.Add(v.Key);
                }
            }
            while (toRemove.Count > 0)
            {
                CoverEffectsToHide.Remove(toRemove[toRemove.Count - 1]);
                toRemove.Remove(toRemove[toRemove.Count - 1]);
            }
            #endregion

            #region Adding effects to covered cells
            List<Vector2Int> lOnlyCovered = OnlyCoveredCells();
            if (ShouldDebug) Debug.Log("Covering " + lOnlyCovered.Count + " cells");
            if (lOnlyCovered.Count <= 0 || lOnlyCovered == null) { continue; }
            foreach (var v in lOnlyCovered)
            {
                CoverType ct = BoardManager.Instance.Board[v.x].Cells[v.y].CoveredBy;

                Cover c = CellCoverManager.Instance.GetCover(ct);
                if (c != null && !CoverEffectsToHide.ContainsKey(v))
                {
                    if (CoverEffectsToHide.ContainsKey(v)) { DestroyToPool(CoverEffectsToHide[v]); CoverEffectsToHide.Remove(v); }

                    List<Vector2Int> lv = new List<Vector2Int> { v };

                    if (ShouldDebug) Debug.Log("Overlaying" + v + " with " + c.Name);
                    GameObject coverOverlay =
                        InstantiateFromPool(c.EffectManagerTag, BoardManager.Instance.BoardToWorldPosition(lv).Value, Quaternion.identity);
                    CoverEffectsToHide.Add(v, coverOverlay);
                }
            }
            #endregion

        }

        if (ShouldDebug) Debug.Log("Stopped to draw covers");
    }

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
                A_Score ability_score = null;
                AbilityInstance aI = v.Key.Get_AbilityInstance(Ability_Name.Score);
                if (aI != null) { ability_score = v.Key.Get_AbilityInstance(Ability_Name.Score).Ability_ as A_Score; }
                 
                if (ability_score != null)
                {
                    int s = ability_score.Check();

                    v.Value.material = ObjMat_Neutral;
                    if (s > 0) { v.Value.material = ObjMat_Player; }
                    if (s < 0) { v.Value.material = ObjMat_Enemy; }
                }              
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

    #region Showing positions of zones (Like bucket's water coverage)
    Coroutine CurZoneShowcase = null;
    public void StartShowingPossibleZone(List<Vector2Int> zoneCoords, Tag coverTag)
    {
        if (CurZoneShowcase == null)
        {
            CurZoneShowcase = StartCoroutine(ShowingPossibleZone_2(zoneCoords, coverTag));
        }
    }
    public void StopShowingPossibleZone()
    {
        if (CurZoneShowcase != null)
        {
            StopCoroutine(CurZoneShowcase);
            CurZoneShowcase = null;
        }

        #region Removing all remaining effects of possible placements
        List<Vector2Int> KeysToDestroy = new List<Vector2Int>();
        foreach (var k in Covers.Keys) { KeysToDestroy.Add(k); }
        while (KeysToDestroy.Count > 0)
        {
            DestroyToPool(Covers[KeysToDestroy[0]]);
            Covers.Remove(KeysToDestroy[0]);
            KeysToDestroy.RemoveAt(0);
        }
        #endregion
    }

    Dictionary<Vector2Int, GameObject> Covers = new Dictionary<Vector2Int, GameObject>();
    IEnumerator ShowingPossibleZone_2(List<Vector2Int> zoneCoords, Tag coverTag)
    {
        yield return new WaitForSeconds(Time.fixedDeltaTime * 0.01f);

        Vector2Int center = BoardManager.Instance.Get_CenterOfZone(zoneCoords);
        foreach (var c in zoneCoords)
        {
            Vector2Int coord = (c - center);
            GameObject g = InstantiateFromPool(coverTag, new Vector3(0, 0, 0), Quaternion.identity);

            Covers.Add(coord, g);
        }
        while (CurZoneShowcase != null && BattleStatsManager.Instance.PlayerTurnActionCondition())
        {
            yield return new WaitForSeconds(Time.fixedDeltaTime * 0.1f);

            foreach (var v in Covers)
            {
                Vector2Int coord = ClickManager.Instance.PointedAtCoords + v.Key;
                if (BoardManager.Instance.IsInBounds(coord))
                {
                    Covers[v.Key].SetActive(true);
                    Covers[v.Key].transform.position = BoardManager.Instance.BoardToWorldPosition(new List<Vector2Int> { coord }).Value;
                }
                else
                {
                    Covers[v.Key].SetActive(false);
                }
            }
        }
            
    }

    #endregion

    #region ShowingPotentialUnitPosition - Coroutine for showing potential new unit position udner cursor within the "availableZone"
    IEnumerator ShowingPotentialUnitPosition(Unit unit, List<Vector2Int> availableZone)
    {
        yield return new WaitForSeconds(Time.fixedDeltaTime * 0.01f);

        RaiseModel(unit);

        while (CurUnitMovePosShowcase != null && BattleStatsManager.Instance.PlayerTurnActionCondition())
        {
            yield return new WaitForSeconds(Time.fixedDeltaTime * 0.1f);

            List<Vector2Int> pos = BoardManager.Instance.ClosestUnitPosToCursor(unit);
            if (availableZone != null && availableZone.Count > 0 && pos != null && pos.Count > 0 &&
                availableZone.Intersect<Vector2Int>(pos).Any())
            {
                unit.UnitModelShowcase.SetActive(true);
                List<Vector2Int> p = new List<Vector2Int> { pos[0] };
                unit.UnitModelShowcase.transform.position = BoardManager.Instance.BoardToWorldPosition(p).Value + unit.ModelOffset;
            }
        }

        LowerModel(unit);
        CurUnitMovePosShowcase = null;
        unit.UnitModelShowcase.SetActive(false);
    }
    #endregion

    #region Showing possible unit movement position under the cursor. Uses "ShowingPotentialUnitPosition"
    Coroutine CurUnitMovePosShowcase;
    void ShowingPotentialMovedPosition_Start(Unit unit)
    {
        if (CurUnitMovePosShowcase == null)
        {
            List<Vector2Int> poss = new List<Vector2Int>();
            foreach (var v in Action_Move.Get_PossibleMovement(unit)) { foreach (var vv in v) { poss.AddRange(vv); } }
            CurUnitMovePosShowcase = StartCoroutine(ShowingPotentialUnitPosition(unit, poss));
        }

    }
    void ShowingPotentialMovedPosition_End(Unit unit)
    {
        LowerModel(unit);

        if (CurUnitMovePosShowcase != null)
        {
            StopCoroutine(CurUnitMovePosShowcase);
            CurUnitMovePosShowcase = null;
        }

        unit.UnitModelShowcase.SetActive(false);
    }
    #endregion

    #region Showing ALL possible movement positions of unit
    void ShowMovement(List<Unit> units)
    {
        if (ShouldDebug) Debug.Log("Showing movement");

        foreach (Unit u in units)
        {
            if (MovementEffectsToHide.ContainsKey(u)) continue;

            //Debug.Log("Called to show possible movement of " + u.gameObject.name);
            List<GameObject> ePu = new List<GameObject>();
            ShowingPotentialMovedPosition_Start(u);
            foreach (var v in Action_Move.Get_PossibleMovement(u))
            {
                foreach (var vv in v)
                {
                    foreach (var vvv in vv)
                    {
                        List<Vector2Int> single = new List<Vector2Int>(); single.Add(vvv);
                        GameObject overlay = InstantiateFromPool(Tag.Placement, BoardManager.Instance.BoardToWorldPosition(single).Value, Quaternion.identity);
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
            ShowingPotentialMovedPosition_End(u);

            foreach (var ef in MovementEffectsToHide[u])
            {
                DestroyToPool(ef);
            }
            MovementEffectsToHide.Remove(u);
        }
    }
    #endregion

    #region Showing ALL possible deployment positions 
    void ShowDeployment(List<Unit> units)
    {
        if (ShouldDebug) Debug.Log("Showing deployment");

        foreach (Unit unit in units)
        {
            if (DeploymentEffectsToHide.ContainsKey(unit)) continue;

            List<GameObject> ePu = new List<GameObject>();

            #region ShowingPotentialUnitPosition under cursor
            if (CurUnitMovePosShowcase == null)
            {
                List<Vector2Int> poss = new List<Vector2Int>();
                foreach (var v in Action_Redeploy.Get_PossibleDeployments(unit)) { foreach (var vv in v) { poss.Add(vv); } }

                CurUnitMovePosShowcase = StartCoroutine(ShowingPotentialUnitPosition(unit, poss));
            }
            #endregion

            #region Going through all deployment zone positions remaining
            List<Vector2Int> zone = new List<Vector2Int>();

            #region Going through all player deployment zones and enabling effects for them
            zone = new List<Vector2Int> { BoardManager.Instance.PlayerDeployment_start, BoardManager.Instance.PlayerDeployment_end };

            for (int x = zone[0].x; x <= zone[1].x; x++)
            {
                for (int y = zone[0].y; y <= zone[1].y; y++)
                {
                    List<Vector2Int> positions = new List<Vector2Int>(); positions.AddRange(unit.Size.Positions);
                    for (int i = 0; i < positions.Count; i++) { positions[i] += new Vector2Int(x, y); }

                    bool isApplicable = true;
                    #region Check if all positions are within the deployment zone
                    foreach (var v in positions)
                    {
                        if (v.x < zone[0].x || v.x > zone[1].x || v.y < zone[0].y || v.y > zone[1].y) { isApplicable = false; break; }
                        if (BoardManager.Instance.Board[x].Cells[y].CurUnit != null) { isApplicable = false; break; }
                    }
                    #endregion

                    if (!isApplicable) continue;

                    foreach (var pos in positions)
                    {
                        List<Vector2Int> p = new List<Vector2Int> { pos };
                        GameObject overlay = InstantiateFromPool(Tag.Placement, BoardManager.Instance.BoardToWorldPosition(p).Value, Quaternion.identity);
                        ePu.Add(overlay);
                    }
                    
                }
            }
            #endregion

            #endregion

            if (!DeploymentEffectsToHide.ContainsKey(unit)) { DeploymentEffectsToHide.Add(unit, ePu); }

        }
    }
    
    void HideDeployment()
    {
        if (ShouldDebug) Debug.Log("Hiding deployment");

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
    #endregion
    
    void Update()
    {
        if (c_DrawingCovers == null)
        {
            c_DrawingCovers = StartCoroutine(DrawingCovers());
        }
    }
}