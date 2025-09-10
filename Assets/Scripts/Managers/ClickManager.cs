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
        if (ShouldDebug) Debug.Log("CanClickOnThings()");

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

        #region If we try to use an item before ending the deployment
        if (BattleStatsManager.Instance.CurRound == 0 && !Action_DrawPlayerResources.IsDeployingNewResources)
        {
            if (ShouldDebug) Debug.Log("Trying to use an item too early ");
            PassTurnButton.Instance.RemindToPass();
        }
        #endregion

        #region Normal behaviour
        else
        {
            E_Click_item.Invoke(i);
        }
        #endregion

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
            ClickHandle(uPos);
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

        if (ShouldDebugCellClicks) Debug.Log("Cell click detected");

        #region Creating a new unit
        if (Action_PlayerCreate.IsWaitingForData /*&& Action_PlayerCreate.CanCreateThere(Action_PlayerCreate.CurUnit, coords)*/)
        {
            if (ShouldDebugCellClicks) { Debug.Log("Creating a unit on " + coords); Debug.Log(ClickBackEvent.GetPersistentEventCount()); }
            ClickBackEvent.Invoke(coords);
        }
        #endregion

        #region Redeploying a player unit in the deployemnt zone
        else if (GameManager.Instance.CurUnitSelected != null && 
            (BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit == null || BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit == GameManager.Instance.CurUnitSelected) 
            && BattleStatsManager.Instance.PlayerTurnActionCondition() && BattleStatsManager.Instance.CurRound <= 0 && !Action_DrawPlayerResources.IsDeployingNewResources)
        {
            if (ShouldDebugCellClicks) Debug.Log("Supposed to redeploy");
            List<Vector2Int> nCoords = new List<Vector2Int> { coords };
            List<Unit> unitToList = new List<Unit> { GameManager.Instance.CurUnitSelected };

            ActionParameters parameters = new ActionParameters(ActionType.Redeploy, unitToList, null, nCoords, null, 0);
            StartCoroutine(GameManager.Instance.Action(parameters));
            GameManager.Instance.CurUnitSelected = null;
        }
        #endregion

        #region Moving a unit to the position
        else if (GameManager.Instance.CurUnitSelected != null
            && (BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit == null || BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit == GameManager.Instance.CurUnitSelected) 
            && BattleStatsManager.Instance.PlayerTurnActionCondition()
            && BattleStatsManager.Instance.CurRound > 0)
        {
            if (ShouldDebugCellClicks) Debug.Log("Moving the unit to position");
            List<Vector2Int> nCoords = new List<Vector2Int>(); nCoords.Add(coords);
            List<Unit> unitToList = new List<Unit>();
            unitToList.Add(GameManager.Instance.CurUnitSelected);

            ActionParameters parameters = new ActionParameters(ActionType.Move, unitToList, null, nCoords, null, 0);
            StartCoroutine(GameManager.Instance.Action(parameters));
            GameManager.Instance.CurUnitSelected = null;
        }
        #endregion

        #region Selecting a new player unit
        else if (BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit != null &&                                   //Must be unit on cooordinates
                BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit.CurKeywords.Contains(Keyword.Player) &&       //Must be player unit
                GameManager.Instance.CurUnitSelected != BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit &&
                !ItemManagement.Instance.IsUsingItem                                                                        //We are not using any items
                )
        {
            if (!Action_DrawPlayerResources.IsDeployingNewResources)
            {
                if (ShouldDebugCellClicks) Debug.Log("Initiating the selectUnit action for " + BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit.name);
                ActionParameters parameters = new ActionParameters(ActionType.SelectUnit, null, null, new List<Vector2Int> { coords }, null, 0);
                StartCoroutine(GameManager.Instance.Action(parameters));
            }
            else
            {
                if (ShouldDebugCellClicks) Debug.Log("Action_DrawPlayerResources.IsDeployingNewResources is "
                    + Action_DrawPlayerResources.IsDeployingNewResources);
            }
        }
        #endregion

        #region Trying to select a unit while we have resources to draw
        else if (BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit != null && Action_DrawPlayerResources.IsDeployingNewResources)
        {
            CameraManager.Instance.LookAtResources();
        }
        #endregion

        else
        {
            if (ShouldDebugCellClicks) Debug.Log("Clicked behaviour not set up");
            ClickBackEvent.Invoke(coords);
        }

        //E_Click_unit.Invoke(null); Not sure if it will be important in the future, null check should exist either way
        E_Click_coords.Invoke(coords);
    }
    #endregion

    #endregion

    #region Continuosly recording what things we are aiming at

    Vector2Int vNull = new Vector2Int(-1, -1);
    [HideInInspector]
    public Vector2Int PointedAtCoords;

    public bool IsPointingAtTurnClock;
    void ContinuousChecking()
    {
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
                        if (uP.Position != null) PointedAtCoords = uP.Position.Value;
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
                        PointedAtCoords = b.Coordinates;
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
                        PointedAtCoords = vNull;
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
                PointedAtCoords = vNull;
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