using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static GameManager;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using System.Runtime.Serialization;
using System;

public class ClickManager : MonoBehaviour
{
    public float ClickCooldown;
    public float ActionToActionSwapCooldown; public float ActionToNoneSwapCooldown;
    public LayerMask LayerMask;
    public bool ShouldDebug;

    float nextClickTime = 1;
    bool SwitchedToAnotherAction;
    bool SwitchedToNoAction;
    public UnityEvent<Vector2Int> ClickBackEvent;

    public static ClickManager Instance;
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

    void Start()
    {

    }

    Transform LastPointedAt = null; public Unit CurrentPointedAtUnit = new Unit();

    #region CellClick
    Vector2Int? CellClick = null;
    #endregion

    #region UnitClick
    [Serializable]
    #nullable enable
    public class UnitP 
    { 
        public Vector2Int? Position = null; 
        public Unit? Unit; 

        public UnitP(Vector2Int p, Unit u) 
        {
            Position = p; 
            Unit = u;
        } 
    }
    UnitP? UnitClick = null;
    Item? ItemClick = null;
    #endregion


    #region Checking if we can click on things
    bool CanClickOnUnits()
    {
        if (ShouldDebug) Debug.Log("CanClickOnUnits()");

        if (nextClickTime >= Time.time) 
        {
            if (ShouldDebug) Debug.Log("Click is on cooldown");
            return false; 
        }

        if (SwitchedToAnotherAction)
        {
            if (ShouldDebug) Debug.Log("Is switching to another action");
            nextClickTime = Time.time + (ActionToActionSwapCooldown * Time.fixedDeltaTime * 100);
            //Debug.Log("Changed: " + nextClickTime + " " + Time.time);
            SwitchedToAnotherAction = false;
            return false;
        }

        if (SwitchedToNoAction)
        {
            if (ShouldDebug) Debug.Log("Is switching to no action");
            nextClickTime = Time.time + (ActionToNoneSwapCooldown * Time.fixedDeltaTime * 100);
            //Debug.Log("Changed: " + nextClickTime + " " + Time.time);
            SwitchedToAnotherAction = false;
            return false;
        }

        if (ShouldDebug) Debug.Log("Can click");
        return true;
    }
    #endregion

    #region Checking if player is hovering over a unit, if they click - this counts as clicking on that unit's cell
    public void Click(InputAction.CallbackContext context)
    {
        if (!CanClickOnUnits()) { return; }
        if (ShouldDebug) Debug.Log("Received an input");

        #region Check what position did we click on and send the click event
        if (CellClick != null)
        {
            nextClickTime = Time.time + (ClickCooldown * Time.fixedDeltaTime * 100);

            if (ShouldDebug) Debug.Log("Launching CellClickHandle");
            ClickHandle(CellClick.Value);

            CellClick = null;
            UnitClick = null;
        }
        else if (UnitClick != null)
        {
            nextClickTime = Time.time + (ClickCooldown * Time.fixedDeltaTime * 100);

            if (ShouldDebug) Debug.Log("Launching CellClickHandle");
            if (UnitClick != null && UnitClick.Unit != null) ClickHandle(UnitClick.Unit);
            CellClick = null;
            UnitClick = null;
        }
        else
        {
            //Debug.Log("Both are null " + UnitClick.Unit + " " + UnitClick.Position);
            //Debug.Log((UnitClick.Unit != NullUnitClick.Unit) + " " + (UnitClick.Position != NullUnitClick.Position));
            //Debug.Log(NullUnitClick.Unit + " " + NullUnitClick.Position);
            
        }

        #endregion

    }
    #endregion


    Coroutine C_ClickCoroutine = null;
    #region Handles when we click on either a unit, cell or an item

    #region Clicking on an item
    public UnityEvent<Item> E_Click_item = new UnityEvent<Item>();
    void ClickHandle(Item i)
    {
        if (ShouldDebug) Debug.Log("ClickHandle on  " + i.Name);


        E_Click_item.Invoke(i);
    }
    #endregion

    #region Clicking on a unit
    public UnityEvent<Unit> E_Click_unit = new UnityEvent<Unit>();
    void ClickHandle(Unit u)
    {
        if (ShouldDebug) Debug.Log("ClickHandle on  " + u.gameObject.name);

        List<Vector2Int> uPoss = BoardManager.Instance.Get_UnitPositions(u);
        #region If unit is on the board
        if (uPoss != null && uPoss.Count > 0) 
        {
            Vector2Int uPos = uPoss[0];
            StartClickCoroutine(uPos);
        }
        #endregion

        E_Click_unit.Invoke(u);
    }
    #endregion

    #region Clicking on a cell
    public UnityEvent<Vector2Int> E_Click_coords = new UnityEvent<Vector2Int>();
    void ClickHandle(Vector2Int coords)
    {
        if (ShouldDebug) Debug.Log("ClickHandle on  " + coords);

        StartClickCoroutine(coords);

        //E_Click_unit.Invoke(null); Not sure if it will be important in the future, null check should exist either way
        E_Click_coords.Invoke(coords);
    }
    #endregion

    #region Coroutine for when we click on a cell/Item
    void StartClickCoroutine(Vector2Int coords)
    {
        if (C_ClickCoroutine == null)
        {
            C_ClickCoroutine = StartCoroutine(CellClickCoroutine(coords));
        }
        else
        {
            StopCoroutine(C_ClickCoroutine);
            C_ClickCoroutine = StartCoroutine(CellClickCoroutine(coords));
        }
    }

    IEnumerator TutorialClickCoroutine(Vector2Int coords)
    {
        yield return new WaitForSeconds(Time.fixedDeltaTime); //For some reason this is vital, otherwise Unity shits itself

        if (TutorialManager.Instance.CanClickOnCells)
        {
            #region Are we selecting a position for creating a unit?
            if (Action_PlayerCreate.IsWaitingForData && Action_PlayerCreate.CanCreateThere(Action_PlayerCreate.Unit, coords))
            #region Yes - Invoke an event to send coordinates where the unit is going to be created (If can be created)
            { //a1
              //Debug.Log("Creating a unit"); 
                if (ShouldDebug) Debug.Log("selecting a position for creating a unit");
                ClickBackEvent.Invoke(coords);
            }
            #endregion

            #region No - Check if there is a unit to move to these coordinates
            else
            {
                #region Do we have a unit and cell is unoccupied?
                if (GameManager.Instance.CurUnitSelected != null && BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit == null &&
                    BattleStatsManager.Instance.PlayerTurnActionCondition())
                #region Yes - Move the unit
                {
                    List<Vector2Int> nCoords = new List<Vector2Int>(); nCoords.Add(coords);
                    List<Unit> unitToList = new List<Unit>(); unitToList.Add(GameManager.Instance.CurUnitSelected);

                    ActionParameters parameters = new ActionParameters(ActionType.Move, unitToList, null, nCoords, null, 0);
                    yield return GameManager.Instance.Action(parameters);
                    GameManager.Instance.CurUnitSelected = null;

                }
                #endregion

                #region No - check the cell to try to interact with a unit on it
                else
                {
                    #region Does this cell have a unit?
                    if (BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit != null)
                    #region Yes - Check what kind of unit this is
                    {
                        #region Is selected unit a player unit we are currently NOT selecting?
                        List<Vector2Int> nCoords = new List<Vector2Int>(); nCoords.Add(coords);
                        if (BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit.CurKeywords.Contains(Keyword.Player)
                            && GameManager.Instance.CurUnitSelected != BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit)
                        #region Yes - select it if we can
                        {
                            #region Safeguard if we are selecting someone else to deploy
                            if (GameManager.Instance.CurrentAction != null &&
                                GameManager.Instance.CurrentAction.Type != ActionType.DeployPlayerStarters)
                            { yield break; }
                            #endregion

                            if (ShouldDebug) Debug.Log("selecting a unit");
                            ActionParameters parameters = new ActionParameters(ActionType.SelectUnit, null, null, nCoords, null, 0);
                            yield return GameManager.Instance.Action(parameters);
                            //else { Debug.Log(ScoreManager.Instance.CurTurn); }
                        }
                        #endregion

                        #region No - TODO:
                        else
                        { //b4)
                            yield return new WaitForSeconds(Time.fixedDeltaTime / 100);
                        }
                        #endregion

                        #endregion
                    }
                    #endregion

                    #region No - Do nothing, clicked on an empty cell
                    else
                    { //b3)
                        Debug.Log("HAVE NOTHING SELECTED, " + coords + " HAS NO UNITS ");
                        //No unit on that cell, do nothing
                    }
                    #endregion
                    #endregion
                }
                #endregion

                #endregion
                #endregion
            }
            #endregion
        }
        else
        {
            Debug.Log("CAN'T CLICK ON CELLS");
        }

        yield return null;
    }

    IEnumerator CellClickCoroutine(Vector2Int coords)
    {
        yield return new WaitForSeconds(Time.fixedDeltaTime); //For some reason this is vital, otherwise Unity shits itself

        if (TutorialManager.Instance != null) { yield return TutorialClickCoroutine(coords); yield break; }

        if (ShouldDebug) Debug.Log("Cell click detected");

        #region Are we selecting a position for creating a unit?
        if (Action_PlayerCreate.IsWaitingForData && Action_PlayerCreate.CanCreateThere(Action_PlayerCreate.Unit, coords))
        #region Yes - Invoke an event to send coordinates where the unit is going to be created (If can be created)
        { //a1
            if (ShouldDebug) Debug.Log("Creating a unit");
            ClickBackEvent.Invoke(coords);
        }
        #endregion

        #region No - Check if there is a unit to move to these coordinates
        else
        {
            if (ShouldDebug) Debug.Log("Not Creating a unit");
            #region Do we have a unit and cell is unoccupied?
            if (GameManager.Instance.CurUnitSelected != null && BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit == null &&
                BattleStatsManager.Instance.PlayerTurnActionCondition())
            #region Yes - Are we in the deployment phase?
            {
                if (ShouldDebug) Debug.Log("Have a unit, cell is unoccupied");
                #region Yes - Redeploy unit to the coordinates
                if (BattleStatsManager.Instance.CurRound <= 0)
                {
                    if (ShouldDebug) Debug.Log("Supposed to redeploy");
                    List<Vector2Int> nCoords = new List<Vector2Int>(); nCoords.Add(coords);
                    List<Unit> unitToList = new List<Unit>();
                    unitToList.Add(GameManager.Instance.CurUnitSelected);

                    ActionParameters parameters = new ActionParameters(ActionType.Redeploy, unitToList, null, nCoords, null, 0);
                    yield return GameManager.Instance.Action(parameters);
                    GameManager.Instance.CurUnitSelected = null;
                }
                #endregion
                #region No - Move unit to the position
                else
                {
                    List<Vector2Int> nCoords = new List<Vector2Int>(); nCoords.Add(coords);
                    List<Unit> unitToList = new List<Unit>();
                    unitToList.Add(GameManager.Instance.CurUnitSelected);

                    ActionParameters parameters = new ActionParameters(ActionType.Move, unitToList, null, nCoords, null, 0);
                    yield return GameManager.Instance.Action(parameters);
                    GameManager.Instance.CurUnitSelected = null;
                }
                #endregion
            }
            #endregion

            #region No - check the cell to try to interact with a unit on it
            else
            {
                if (ShouldDebug) Debug.Log("Don't have a unit, interacting with the unit");
                #region Does this cell have a unit?
                if (BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit != null)
                #region Yes - Check what kind of unit this is
                {
                    if (ShouldDebug) Debug.Log("Cell has a unit");
                    #region Is selected unit a PLAYER unit we are currently NOT selecting?
                    List<Vector2Int> nCoords = new List<Vector2Int>(); nCoords.Add(coords);
                    if (BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit.CurKeywords.Contains(Keyword.Player)
                        && GameManager.Instance.CurUnitSelected != BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit)
                    #region Yes - select it if we can
                    {
                        if (ShouldDebug) Debug.Log("Initiating the selectUnit action");
                        ; ActionParameters parameters = new ActionParameters(ActionType.SelectUnit, null, null, nCoords, null, 0);
                        yield return GameManager.Instance.Action(parameters);
                        //else { Debug.Log(ScoreManager.Instance.CurTurn); }
                    }
                    #endregion

                    #region No - TODO:
                    else
                    { //b4)
                        if (ShouldDebug) Debug.Log("Clicking on a non-player unit or unit is already selected");
                        GameManager.Instance.CurUnitSelected = null;
                        ActionParameters parameters = new ActionParameters(ActionType.SelectUnit, null, null, nCoords, null, 0);
                        yield return GameManager.Instance.Action(parameters);
                    }
                    #endregion

                    #endregion
                }
                #endregion

                #region No - Do nothing, clicked on an empty cell
                else
                { //b3)
                    if (ShouldDebug) Debug.Log("HAVE NOTHING SELECTED, " + coords + " HAS NO UNITS ");
                    //No unit on that cell, do nothing
                }
                #endregion
                #endregion
            }
            #endregion

            #endregion
        }
        #endregion

        #endregion

        yield return null;
    }
    #endregion

    #endregion

    #region Continuosly recording what things we are aiming at
    bool IsPointingAtTurnClock;
    void ContinuousChecking()
    {
        //if (!CanClickOnUnits()) return;

        RaycastHit hit;
        Vector3 vect = Input.mousePosition;
        vect.z = 999999;
        Vector3 cPos = Camera.main.ScreenToWorldPoint(vect);
        Physics.Raycast(Camera.main.transform.position, cPos, out hit, Mathf.Infinity, LayerMask);

        #region If hit something
        if (hit.transform != null)
        {
            #region If hit a unit
            if (hit.transform.tag.ToLower() == "unit")
            {
                if (ShouldDebug) Debug.Log(hit.transform.gameObject.name + " unit was hit with raycast");
                #region If keep pointing at the same unit
                if (hit.transform == LastPointedAt)
                {
                    if (ShouldDebug) Debug.Log(hit.transform.gameObject.name + " is the same unit, changing nothing");
                    if (ShouldDebug && UnitClick != null) Debug.Log(UnitClick.Unit + " " + UnitClick.Position);

                    ItemClick = null;
                    IsPointingAtTurnClock = false;
                }
                #endregion
                #region If hover over a new unit
                else
                {
                    Debug.Log(hit.transform.gameObject.name + " is new, making current");
                    LastPointedAt = hit.transform;
                    Unit u = LastPointedAt.GetComponent<Unit>();
                    CurrentPointedAtUnit = u;
                    if (CurrentPointedAtUnit != null)
                    {
                        if (ShouldDebug) Debug.Log("Found a new unit: " + CurrentPointedAtUnit.gameObject.name);

                        List<Vector2Int> p = BoardManager.Instance.Get_UnitPositions(u);
                        CellClick = null;

                        UnitP uP = new UnitP(new Vector2Int(0, 0), CurrentPointedAtUnit);
                        if (p != null && p.Count > 0) { uP.Position = p[0]; }
                        else { uP.Position = null; }

                        UnitClick = uP;
                        IsPointingAtTurnClock = false;
                        ItemClick = null;


                    }
                }
                #endregion
            }
            #endregion
            #region If hit a cell
            else if (hit.transform.tag.ToLower() == "cell")
            {
                //if (ShouldDebug) Debug.Log(hit.transform.gameObject.name + " cell was hit with raycast");

                #region If hover and click on an old cell
                if (hit.transform == LastPointedAt)
                {
                    IsPointingAtTurnClock = false;
                }
                #endregion
                #region If hover over a new cell
                else
                {
                    LastPointedAt = hit.transform;
                    BoardCell b = LastPointedAt.GetComponent<BoardCell>();
                    if (b != null)
                    {
                        UnitClick = null; //This Might break
                        CellClick = b.Coordinates;
                        ItemClick = null;

                        if (ShouldDebug) Debug.Log("Found a new unit: " + CellClick);
                        IsPointingAtTurnClock = false;
                    }
                }
                #endregion
            }
            #endregion
            #region If hit an item
            else if (hit.transform.tag.ToLower() == "item")
            {
                if (ShouldDebug) Debug.Log(hit.transform.gameObject.name + " item was hit with raycast");

                #region If hover and click on an old cell
                if (hit.transform == LastPointedAt)
                {
                    IsPointingAtTurnClock = false;
                }
                #endregion
                #region If hover over a new cell
                else
                {
                    LastPointedAt = hit.transform;
                    Item i = LastPointedAt.GetComponent<Item>();
                    if (i != null)
                    {
                        UnitClick = null; //This Might break
                        CellClick = null;
                        ItemClick = i;
                        IsPointingAtTurnClock = false;

                        if (ShouldDebug) Debug.Log("Found a new item: " + i.Name);
                    }
                }
                #endregion
            }
            #endregion

            #region If hit a turn clock
            else if (hit.transform.tag.ToLower() == "turnclock")
            {
                IsPointingAtTurnClock = true;

                if (ShouldDebug) Debug.Log(hit.transform.gameObject.name + " turn clock was hit with raycast");
                PassTurnButton.Instance.Pass();
            }
            #endregion

        }
        #endregion
        else
        {
            if (ShouldDebug) Debug.Log("Raycast hit nothing");
        }
    }
    #endregion

    void Update()
    {
        ContinuousChecking();
    }
}