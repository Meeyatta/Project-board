using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

//Main script, handles all of the actions what can be done with units
public class GameManager : MonoBehaviour
{
    

    public enum AcionType { Move, Select, ForcedMove };
    public Unit CurUnitSelected; //What unit is currently selected, if no unit - should be null
    public static GameManager Instance;
    public Coroutine GoingThroughActions;
    public Coroutine SelectCoroutine;
    public Dictionary<IEnumerator, string> ActionQueue = new Dictionary<IEnumerator, string>();

    #region Events 
    public UnityEvent<List<Unit>> ShowMovementEvent;
    public UnityEvent<List<Unit>> HideMovementEvent;
    void OnEnable()
    {
        
    }
    void OnDisable()
    {
        
    }
    #endregion Events

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
    
    private void Start()
    {

    }

    //Can be called by a variety of things to a variety of actions. PLEASE REMEMBER TO PUT IN BRACKETS WHAT INFO IS NEEDED TO DO THE ACTION
    public IEnumerator Action(AcionType type, Unit ActionTargetUnit , List<Vector2Int> CellsCoordinates)
    {
        switch (type)
        {
            //Move the unit to the coordinates if it can move there with it's moveset, Requires: (ActionTargetUnit, CellsCoordinates)
            #region Move(ActionTargetUnit, CellsCoordinates)
            case AcionType.Move:
                ActionQueue.Add( Move(ActionTargetUnit, CellsCoordinates), "Move");             
                break;
            #endregion Move(ActionTargetUnit, CellsCoordinates)

            //Set a unit under coordinates as a Current selected unit by the player, Requires: (CellsCoordinates)
            //  This one should be called with StartCoroutine instead of yield return, because unit can be selected while other actions are done
            #region Select(CellsCoordinates)
            case AcionType.Select:
                SelectCoroutine = StartCoroutine( Select(CellsCoordinates) );
                break;
            #endregion Select(CellCoordinates)

            //Move the unit to the coordinates, doesn't check for unit's moveset Requires: (ActionTargetUnit, CellsCoordinates)
            #region ForcedMove(ActionTargetUnit, CellsCoordinates)
            case AcionType.ForcedMove:
                ActionQueue.Add(Move(ActionTargetUnit, CellsCoordinates), "Move");
                break;
            #endregion Move(ActionTargetUnit, CellsCoordinates)

            //Means I forgot to make an action for this type
            #region Default(...)
            default:
                Debug.LogError("Action not written");
                break;
            #endregion Default(...)
        }
        yield return null;
    }
        IEnumerator Move(Unit ActionTargetUnit, List<Vector2Int> CellsCoordinates)
        {
            if (ActionTargetUnit == null || CellsCoordinates.Count == 0) Debug.LogError("INVALID ACTION PARAMETERS - MOVE(ActionTargetUnit, CellCoordinates)");

            List<Vector2Int> possiblePosses = new List<Vector2Int>();

            #region Setup Possible positionds from unit's moveset
            foreach (Vector2Int v in BoardManager.Instance.Get_UnitPositions(ActionTargetUnit))
                {
                    foreach (var vl in ActionTargetUnit.CurMoveset.Lines)
                    {
                        foreach (Vector2Int vv in vl.Positions)
                        {
                            possiblePosses.Add(v + vv);
                        }
                    }
                }
            #endregion
            //Stopped working at this
            #region Remove positions obscured by obstacles
            foreach (var line in ActionTargetUnit.CurMoveset.Lines)
            {
                for (int i = line.Positions.Count-1; i > 0; i--)
                {
                    List<Vector2Int> curI = new List<Vector2Int>(); curI.Add(line.Positions[i]);
                    if (BoardManager.Instance.AreCellsOccupied(curI))
                    {
                        for (int ii = i; ii < line.Positions.Count; ii++)
                        {
                            foreach (Vector2Int unitPos in BoardManager.Instance.Get_UnitPositions(ActionTargetUnit))
                            {
                                possiblePosses.Remove(unitPos + line.Positions[ii]);
                            }
                        }
                    }
                }
            }
            #endregion
            Debug.Log(BoardManager.Instance.AreCellsOccupied(possiblePosses));
        

            bool both = possiblePosses.Intersect(CellsCoordinates).Any();
            if (both)
            {
                yield return StartCoroutine(BoardManager.Instance.MoveUnit(ActionTargetUnit, CellsCoordinates));
                Debug.Log("HAS BEEN MOVED TO " + CellsCoordinates);
            }

            yield return new WaitForSeconds(0.001f);
        }
        IEnumerator Select(List<Vector2Int> CellsCoordinates)
        {
            if (CellsCoordinates.Count == 0) Debug.LogError("INVALID ACTION PARAMETERS - SELECT(CellCoordinates)");
            
            Vector2Int coords = CellsCoordinates[0];
            Debug.Log("SELECTED ON" + coords);

            CurUnitSelected = BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit;
            Unit ogUnit = CurUnitSelected;

            List<Unit> us = new List<Unit>(); us.Add(CurUnitSelected);
            ShowMovementEvent.Invoke(us);   

            yield return new WaitForSeconds(0.01f);
            while (CurUnitSelected != null && ogUnit == CurUnitSelected)
            {
                Debug.Log("IS SELECTING A UNIT");
                yield return new WaitForSeconds(0.01f);
            }
            HideMovementEvent.Invoke(us);
        }
        IEnumerator ForcedMove(Unit ActionTargetUnit, List<Vector2Int> CellsCoordinates)
        {
            if (ActionTargetUnit == null || CellsCoordinates.Count == 0) Debug.LogError("INVALID ACTION PARAMETERS - MOVE(ActionTargetUnit, CellCoordinates)");


            yield return StartCoroutine(BoardManager.Instance.MoveUnit(ActionTargetUnit, CellsCoordinates));
            Debug.Log("MOVED TO " + CellsCoordinates);

            yield return new WaitForSeconds(0.001f);
        }
    public void CellClickHandle(Vector2Int coords)
    {
        Debug.Log("SOMEONE CLICKED THE CELL ON " + coords);
        StartCoroutine(CellClickCoroutine(coords));
    }
    IEnumerator CellClickCoroutine(Vector2Int coords)
    {
        yield return new WaitForSeconds(0.001f); //For some reason this is vital, otherwise Unity shits itself trying to assign and end a Coroutine at the same time 

        if (CurUnitSelected != null && BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit == null) //If have a unit and cell is unoccupied - a)move it to the cell, otherwise - b)select a unit on that cell
        { //a)

            List<Vector2Int> nCoords = new List<Vector2Int>(); nCoords.Add(coords);
            yield return StartCoroutine(Action(AcionType.Move, CurUnitSelected, nCoords));
            CurUnitSelected = null;

        }
        else
        { //b)
            if (BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit != null) //Check whenever there is a unit on a clicked cell
            {
                List<Vector2Int> nCoords = new List<Vector2Int>(); nCoords.Add(coords);

                yield return StartCoroutine(Action(AcionType.Select, null, nCoords));
            }
            else
            {
                Debug.Log("HAVE NOTHING SELECTED, " + coords + " HAS NO UNITS ");
                //No unit on that cell, do nothing
            }
        }
        yield return null;
    }
    private void FixedUpdate()
    {
        while (ActionQueue.Count > 0 && GoingThroughActions == null)
        {
            GoingThroughActions = StartCoroutine(GoThroughActions());
        }
    }
    IEnumerator GoThroughActions()
    {
        yield return new WaitForSeconds(0.0001f);

        foreach (var coroutine in ActionQueue)
        {
            yield return StartCoroutine(coroutine.Key);
            
        }
        ActionQueue.Clear();
        GoingThroughActions = null;
    }

    private void Update()
    {
        if (Input.GetKeyDown("q"))
        {
            Debug.Log("CURRENT ACTION QUEUE:");
            foreach (var a in ActionQueue) { Debug.Log(a.Value); }

        }
    }
}
