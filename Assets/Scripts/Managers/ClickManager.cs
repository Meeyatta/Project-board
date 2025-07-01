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
    public bool ShouldDebugCellClicks;
    public bool ShouldDebugRaycast;

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

    Transform LastPointedAt = null; 
    public Unit CurrentPointedAtUnit = new Unit();
    public Item CurrentPointedAtItem = new Item();

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
    bool CanClickOnThings()
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
        if (!CanClickOnThings()) { return; }
        if (ShouldDebug) Debug.Log("Received an input");

        #region Check what position did we click on and send the click event
        if (CellClick != null)
        {
            nextClickTime = Time.time + (ClickCooldown * Time.fixedDeltaTime * 100);

            if (ShouldDebug) Debug.Log("Launching CellClickHandle");
            ClickHandle(CellClick.Value);

            ItemClick = null;
            CellClick = null;
            UnitClick = null;
        }
        else if (UnitClick != null)
        {
            nextClickTime = Time.time + (ClickCooldown * Time.fixedDeltaTime * 100);

            if (ShouldDebug) Debug.Log("Launching UnitClickHandle");
            if (UnitClick.Unit != null) ClickHandle(UnitClick.Unit);

            ItemClick = null;
            CellClick = null;
            UnitClick = null;
        }
        else if (ItemClick != null)
        {
            nextClickTime = Time.time + (ClickCooldown * Time.fixedDeltaTime * 100);

            if (ShouldDebug) Debug.Log("Launching Item ClickHandle");
            ClickHandle(ItemClick);

            ItemClick = null;
            CellClick = null;
            UnitClick = null;

        }
        else if (IsPointingAtTurnClock)
        {
            PassTurnButton.Instance.Pass();

            ItemClick = null;
            CellClick = null;
            UnitClick = null;
            IsPointingAtTurnClock = false;
        }

        else
        {
            //Debug.Log("ItemClick:" + ItemClick + " CellClick:" + CellClick + " UnitClick:" + UnitClick + " IsPointingAtTurnClock:" + IsPointingAtTurnClock);
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
        if (ShouldDebug) Debug.Log("Item ClickHandle on " + i.Name);
        E_Click_item.Invoke(i);
    }
    #endregion

    #region Clicking on a unit
    public UnityEvent<Unit> E_Click_unit = new UnityEvent<Unit>();
    void ClickHandle(Unit u)
    {
        if (ShouldDebug) Debug.Log("Unit ClickHandle on " + u.gameObject.name);

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
        //if (ShouldDebug) Debug.Log("ClickHandle on  " + coords);

        StartClickCoroutine(coords);

        //E_Click_unit.Invoke(null); Not sure if it will be important in the future, null check should exist either way
        E_Click_coords.Invoke(coords);
    }
    #endregion

    #region Coroutine for when we click on a cell
    void StartClickCoroutine(Vector2Int coords)
    {
        if (C_ClickCoroutine == null)
        {
            C_ClickCoroutine = StartCoroutine(CellClickCoroutine_2(coords));
        }
        else
        {
            StopCoroutine(C_ClickCoroutine);
            C_ClickCoroutine = StartCoroutine(CellClickCoroutine_2(coords));
        }
    }

    IEnumerator TutorialClickCoroutine(Vector2Int coords)
    {
        yield return new WaitForSeconds(Time.fixedDeltaTime); //For some reason this is vital, otherwise Unity shits itself

        if (TutorialManager.Instance.CanClickOnCells)
        {
            #region Are we selecting a position for creating a unit?
            if (Action_PlayerCreate.IsWaitingForData && Action_PlayerCreate.CanCreateThere(Action_PlayerCreate.CurUnit, coords))
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

    //New trial version where each click have individual conditions
    IEnumerator CellClickCoroutine_2(Vector2Int coords)
    {
        yield return new WaitForSeconds(Time.fixedDeltaTime);

        if (ShouldDebugCellClicks) Debug.Log("Cell click detected");

        #region Creating a new unit
        if (Action_PlayerCreate.IsWaitingForData /*&& Action_PlayerCreate.CanCreateThere(Action_PlayerCreate.CurUnit, coords)*/)
        {
            if (ShouldDebugCellClicks) Debug.Log("Creating a unit");
            ClickBackEvent.Invoke(coords);
        }
        #endregion

        #region Redeploying a player unit in the deployemnt zone
        else if (GameManager.Instance.CurUnitSelected != null && BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit == null && BattleStatsManager.Instance.PlayerTurnActionCondition()
            && BattleStatsManager.Instance.CurRound <= 0 && !Action_DrawPlayerResources.IsDeployingNewResources)
        {
            if (ShouldDebugCellClicks) Debug.Log("Supposed to redeploy");
            List<Vector2Int> nCoords = new List<Vector2Int>(); nCoords.Add(coords);
            List<Unit> unitToList = new List<Unit>();
            unitToList.Add(GameManager.Instance.CurUnitSelected);

            ActionParameters parameters = new ActionParameters(ActionType.Redeploy, unitToList, null, nCoords, null, 0);
            yield return GameManager.Instance.Action(parameters);
            GameManager.Instance.CurUnitSelected = null;
        }
        #endregion

        #region Moving a unit to the position
        else if (GameManager.Instance.CurUnitSelected != null && BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit == null && BattleStatsManager.Instance.PlayerTurnActionCondition()
            && BattleStatsManager.Instance.CurRound > 0)
        {
            if (ShouldDebugCellClicks) Debug.Log("Moving the unit to position");
            List<Vector2Int> nCoords = new List<Vector2Int>(); nCoords.Add(coords);
            List<Unit> unitToList = new List<Unit>();
            unitToList.Add(GameManager.Instance.CurUnitSelected);

            ActionParameters parameters = new ActionParameters(ActionType.Move, unitToList, null, nCoords, null, 0);
            yield return GameManager.Instance.Action(parameters);
            GameManager.Instance.CurUnitSelected = null;
        }
        #endregion

        #region Selecting a new player unit
        else if (BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit != null && 
            BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit.CurKeywords.Contains(Keyword.Player) && GameManager.Instance.CurUnitSelected != BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit)         
        {
            if (!Action_DrawPlayerResources.IsDeployingNewResources)
            {
                if (ShouldDebugCellClicks) Debug.Log("Initiating the selectUnit action for " + BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit.name);
                ActionParameters parameters = new ActionParameters(ActionType.SelectUnit, null, null, new List<Vector2Int> { coords }, null, 0);
                yield return GameManager.Instance.Action(parameters);
            }
            else
            {
                if (ShouldDebugCellClicks) Debug.Log("Action_DrawPlayerResources.IsDeployingNewResources is "
                    + Action_DrawPlayerResources.IsDeployingNewResources);
            }
        }
        #endregion

        else
        {
            if (ShouldDebugCellClicks) Debug.Log("Clicked on nothing");
            ClickBackEvent.Invoke(coords);
        }
    }

    IEnumerator CellClickCoroutine_deprecated(Vector2Int coords)
    {
        yield return new WaitForSeconds(Time.fixedDeltaTime); //For some reason this is vital, otherwise Unity shits itself

        if (TutorialManager.Instance != null) { yield return TutorialClickCoroutine(coords); yield break; }

        if (ShouldDebugCellClicks) Debug.Log("Cell click detected");

        #region Are we selecting a position for creating a unit?
        if (Action_PlayerCreate.IsWaitingForData && Action_PlayerCreate.CanCreateThere(Action_PlayerCreate.CurUnit, coords))
        #region Yes - Invoke an event to send coordinates where the unit is going to be created (If can be created)
        { //a1
            if (ShouldDebugCellClicks) Debug.Log("Creating a unit");
            ClickBackEvent.Invoke(coords);
        }
        #endregion

        #region No - Check if there is a unit to move to these coordinates
        else
        {
            if (ShouldDebugCellClicks) Debug.Log("Not Creating a unit");
            #region Do we have a unit and cell is unoccupied?
                if (GameManager.Instance.CurUnitSelected != null && BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit == null &&
                    BattleStatsManager.Instance.PlayerTurnActionCondition())  
                {
                    #region Yes - Are we in the deployment phase?
                    if (ShouldDebugCellClicks) Debug.Log("Have a unit, cell is unoccupied");
                    if (BattleStatsManager.Instance.CurRound <= 0)
                    {
                        #region Yes - Are we currently deploying any units? 
                        if (Action_DrawPlayerResources.IsDeployingNewResources)
                        {
                            #region Yes - do nothing
                            if (ShouldDebugCellClicks) Debug.Log("Tried to redeploy, but currently deploying resources");
                            #endregion
                        }
                        else
                        {
                            #region No - Redeploy unit to the coordinates
                            if (ShouldDebugCellClicks) Debug.Log("Supposed to redeploy");
                            List<Vector2Int> nCoords = new List<Vector2Int>(); nCoords.Add(coords);
                            List<Unit> unitToList = new List<Unit>();
                            unitToList.Add(GameManager.Instance.CurUnitSelected);

                            ActionParameters parameters = new ActionParameters(ActionType.Redeploy, unitToList, null, nCoords, null, 0);
                            yield return GameManager.Instance.Action(parameters);
                            GameManager.Instance.CurUnitSelected = null;
                            #endregion
                        }         
                        #endregion
                    }
                    else
                    {
                        #region No - Move unit to the position
                        if (ShouldDebugCellClicks) Debug.Log("Moving the unit to position");
                        List<Vector2Int> nCoords = new List<Vector2Int>(); nCoords.Add(coords);
                        List<Unit> unitToList = new List<Unit>();
                        unitToList.Add(GameManager.Instance.CurUnitSelected);

                        ActionParameters parameters = new ActionParameters(ActionType.Move, unitToList, null, nCoords, null, 0);
                        yield return GameManager.Instance.Action(parameters);
                        GameManager.Instance.CurUnitSelected = null;
                    #endregion
                }
                #endregion
                }

            #region No - check the cell to try to interact with a unit on it
            else
                {
                    if (ShouldDebugCellClicks) Debug.Log("Don't have a unit, interacting with the unit");
                    #region Does this cell have a unit?
                    if (BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit != null)
                    #region Yes - Check what kind of unit this is
                    {
                        if (ShouldDebugCellClicks) Debug.Log("Cell has a unit");
                        #region Is selected unit a PLAYER unit we are currently NOT selecting?
                        List<Vector2Int> nCoords = new List<Vector2Int>(); nCoords.Add(coords);
                        if (BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit.CurKeywords.Contains(Keyword.Player)
                            && GameManager.Instance.CurUnitSelected != BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit)
                        #region Yes - can we select it?
                        {
                            if (!Action_DrawPlayerResources.IsDeployingNewResources)
                            #region Yes
                            {
                                if (ShouldDebugCellClicks) Debug.Log("Initiating the selectUnit action");
                                ActionParameters parameters = new ActionParameters(ActionType.SelectUnit, null, null, nCoords, null, 0);
                                yield return GameManager.Instance.Action(parameters);
                            }
                            #endregion
                            #region No
                            else
                            {
                            if (ShouldDebugCellClicks) Debug.Log("Action_DrawPlayerResources.IsDeployingNewResources is " 
                                + Action_DrawPlayerResources.IsDeployingNewResources);
                            }
                            #endregion  
                            
                        }
                        #endregion

                        #region No - TODO:
                        else
                        { //b4)
                            if (ShouldDebugCellClicks) Debug.Log("Clicking on a non-player unit or unit is already selected");
                            //GameManager.Instance.CurUnitSelected = null;
                            //ActionParameters parameters = new ActionParameters(ActionType.SelectUnit, null, null, nCoords, null, 0);
                            //yield return GameManager.Instance.Action(parameters);
                        }
                        #endregion

                        #endregion
                    }
                    #endregion

                    #region No - Do nothing, clicked on an empty cell
                    else
                    { //b3)
                        if (ShouldDebugCellClicks) Debug.Log("HAVE NOTHING SELECTED, " + coords + " HAS NO UNITS ");
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
    public bool IsPointingAtTurnClock;
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
                //if (ShouldDebug) Debug.Log(hit.transform.gameObject.name + " unit was hit with raycast");

                CurrentPointedAtItem = null;
                #region If keep pointing at the same unit
                if (hit.transform == LastPointedAt)
                {
                    //if (ShouldDebug) Debug.Log(hit.transform.gameObject.name + " is the same unit, changing nothing");
                    //if (ShouldDebug && UnitClick != null) Debug.Log(UnitClick.Unit + " " + UnitClick.Position);

                    ItemClick = null;
                    IsPointingAtTurnClock = false;
                }
                #endregion
                #region If hover over a new unit
                else
                {
                    if (ShouldDebugRaycast) Debug.Log(hit.transform.gameObject.name + " is new, making current");
                    LastPointedAt = hit.transform;
                    Unit u = LastPointedAt.GetComponent<Unit>();
                    CurrentPointedAtUnit = u;
                    if (CurrentPointedAtUnit != null)
                    {
                        if (ShouldDebugRaycast) Debug.Log("Found a new unit: " + CurrentPointedAtUnit.gameObject.name);

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
                if (ShouldDebugRaycast) Debug.Log(hit.transform.gameObject.name + " cell was hit with raycast");

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

                        //if (ShouldDebug) Debug.Log("Found a new unit: " + CellClick);
                        IsPointingAtTurnClock = false;
                    }
                }
                #endregion
            }
            #endregion
            #region If hit an item
            else if (hit.transform.tag.ToLower() == "item")
            {
                if (ShouldDebugRaycast) Debug.Log(hit.transform.gameObject.name + " item was hit with raycast");

                CurrentPointedAtUnit = null;
                #region If hover and click on an old item
                if (hit.transform == LastPointedAt)
                {
                    IsPointingAtTurnClock = false;
                }
                #endregion
                #region If hover over a new item
                else
                {
                    LastPointedAt = hit.transform;
                    Item i = LastPointedAt.GetComponent<Item>();
                    CurrentPointedAtItem = i;
                    if (i != null)
                    {
                        UnitClick = null; //This Might break
                        CellClick = null;
                        ItemClick = i;
                        IsPointingAtTurnClock = false;

                        //if (ShouldDebug) Debug.Log("Found a new item: " + i.Name);
                    }
                }
                #endregion
            }
            #endregion

            #region If hit a turn clock
            else if (hit.transform.tag.ToLower() == "turnclock")
            {
                IsPointingAtTurnClock = true;

                CurrentPointedAtUnit = null;
                CurrentPointedAtItem = null;
                if (ShouldDebugRaycast) Debug.Log(hit.transform.gameObject.name + " turn clock was hit with raycast");
                //PassTurnButton.Instance.Pass();
            }
            #endregion

            else
            {
                if (ShouldDebugRaycast) Debug.Log("Hit something else: " + hit.transform.gameObject.name + " with tag " + hit.transform.tag);
                Reset_CanClickOnNothing();
            }

        }
        #endregion
        else
        {
            if (ShouldDebug) Debug.Log("Raycast hit nothing");
            Reset_CanClickOnNothing();
        }
    }
    #endregion

    void Reset_CanClickOnNothing()
    {
        LastPointedAt = null;
        CurrentPointedAtUnit = null;
        UnitClick = null;

        CellClick = null;

        CurrentPointedAtItem = null;
        ItemClick = null;
        IsPointingAtTurnClock = false;
    }

    void Update()
    {
        ContinuousChecking();
    }
}