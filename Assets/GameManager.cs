using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

//Main script, handles all of the actions what can be done with units

/*
    LIST OF ACTIONS:
        Move(ActionTargetUnits, CellsCoordinates) - Move the unit to the coordinates if it can move there with it's moveset, Requires: (ActionTargetUnit, CellsCoordinates)
        Select(CellsCoordinates) - Set a unit under coordinates as a Current selected unit by the player, Requires: (CellsCoordinates) 
        ForcedMove(ActionTargetUnits, CellsCoordinates) - Move the unit to the coordinates, doesn't check for unit's moveset Requires: (ActionTargetUnit, CellsCoordinates)
        Attack(ActionTargetUnits) - Make a target unit initiate an attack on all units in it's attack zone
        KeywordedAttack(Keywords) - Make all units with specific keywords initiate an attack on all units in their individual attack zones

*/

/*
    LIST OF USEFUL FUNCTIONS:
        List<List<Vector2Int>> GetPossibleMovement(Unit unit) - Returns all possible positions what a unit can move using their current muveset    
        List<Unit> GetPossibleTargets(Unit source, List<Unit.Keyword> keywords) - Returns all possible units what a source (unit) can affect using their current attack Zones
        bool UnitHasAllKeywords(Unit u, List<Unit.Keyword> keywords) - returns true if unit has all of the keywords in "keywords"
        List<Unit> GroupUnitsByKeywords(List<Unit> all, List<Unit.Keyword> keywords) - Groups units from a list into a different list only if they have the keywords in "keywords"
    
 
*/

public class GameManager : MonoBehaviour
{
    public class ActionSlot
    {
        public IEnumerator IEnum;
        public string Name;

        public ActionSlot(IEnumerator e, string n)
        {
            IEnum = e;
            Name = n;
        }
    }

    public enum AcionType { Attack, KeywordedAttack ,Move, Select, ForcedMove };
    public Unit CurUnitSelected; //What unit is currently selected, if no unit - should be null
    public static GameManager Instance;
    public Coroutine GoingThroughActions;
    public Coroutine SelectCoroutine;
    public Queue<ActionSlot> ActionQueue = new Queue<ActionSlot>();

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

    //Can be called by a variety of things to initiate a variety of actions. PLEASE REMEMBER TO PUT IN BRACKETS WHAT INFO IS NEEDED TO DO THE ACTION
        /*TODO : Separate this function into several smaller ones with different required parameters, since currently the function has a lot of parametres used only by a singluar type of action,
        which is messy and needs a bunch of null statements */
    public IEnumerator Action(AcionType type, List<Unit> ActionTargetUnits, List<Unit.Keyword> keywords,  List<Vector2Int> CellsCoordinates)
    {
        switch (type)
        {
            //Move the unit to the coordinates if it can move there with it's moveset, Requires: (ActionTargetUnit, CellsCoordinates)
            #region Move(ActionTargetUnits, CellsCoordinates)
            case AcionType.Move:
                ActionSlot move = new ActionSlot(Action_Move.Instance.Move(ActionTargetUnits[0], CellsCoordinates), "Move");
                ActionQueue.Enqueue(move);             
                break;
            #endregion Move(ActionTargetUnits, CellsCoordinates)

            //Set a unit under coordinates as a Current selected unit by the player, Requires: (CellsCoordinates)
            //  This one should be called with StartCoroutine instead of yield return, because unit can be selected while other actions are done
            #region Select(CellsCoordinates)
            case AcionType.Select:
                SelectCoroutine = StartCoroutine( Action_Select.Instance.Select(CellsCoordinates) );
                break;
            #endregion Select(CellCoordinates)

            //Move the unit to the coordinates, doesn't check for unit's moveset Requires: (ActionTargetUnit, CellsCoordinates)
            //TODO: Separate into a unique script, currently the corotuine in inside GameManager
            #region ForcedMove(ActionTargetUnits, CellsCoordinates)
            case AcionType.ForcedMove:
                ActionSlot forcedmove = new ActionSlot(ForcedMove(ActionTargetUnits[0], CellsCoordinates), "Forced Move");
                ActionQueue.Enqueue(forcedmove);
                break;
            #endregion ForcedMove(ActionTargetUnits, CellsCoordinates)

            //Make a target unit initiate an attack on all units in it's attack zone
            #region Attack(ActionTargetUnits)
            case AcionType.Attack:
                ActionSlot attack = new ActionSlot(Action_Attack.Instance.Attack(ActionTargetUnits), "Attack");
                ActionQueue.Enqueue(attack);
                break;
            #endregion Attack(ActionTargetUnits)

            //Make all units with specific keywords initiate an attack on all units in their individual attack zones
            #region KeywordedAttack(Keywords)
            case AcionType.KeywordedAttack:
                ActionSlot keywordedattack = new ActionSlot(Action_Attack.Instance.Attack(GroupUnitsByKeywords(BoardManager.Instance.Get_AllUnitsOnBoard(), keywords)), "Attack");
                ActionQueue.Enqueue(keywordedattack);
                break;
            #endregion KeywordedAttack(Keywords)

            //Means I forgot to make an action for this type
            #region Default(...)
            default:
                Debug.LogError("Action not written");
                break;
            #endregion Default(...)
        }
        yield return null;
    }

        IEnumerator ForcedMove(Unit ActionTargetUnit, List<Vector2Int> CellsCoordinates)
        {
            if (ActionTargetUnit == null || CellsCoordinates.Count == 0) Debug.LogError("INVALID ACTION PARAMETERS - MOVE(ActionTargetUnit, CellCoordinates)");


            yield return StartCoroutine(BoardManager.Instance.MoveUnit(ActionTargetUnit, CellsCoordinates));
            //Debug.Log("MOVED TO " + CellsCoordinates);

            yield return new WaitForSeconds(0.001f);
        }

    //Returns all possible positions what a unit can move using their current muveset    
    public List<List<Vector2Int>> GetPossibleMovement(Unit unit)
        {
           // Debug.Log("Possible movement positions:");
            List<List<Vector2Int>> res = new List<List<Vector2Int>>();

            #region Checking if a line crosses through another unit
            foreach (CellGuide.Line line in unit.CurMoveset.Lines)
                {
                    for (int i = 0; i < line.Positions.Count; i++)
                    {
                        string lineS = "";
                        List<Vector2Int> iPositions = new List<Vector2Int>();
                        foreach (Vector2Int unitP in BoardManager.Instance.Get_UnitPositions(unit))
                        {
                            if (!BoardManager.Instance.IsInBounds(line.Positions[i] + unitP)) { break; }
                            int x = line.Positions[i].x; int y = line.Positions[i].y;
                            if (BoardManager.Instance.Board[unitP.x + x].Cells[unitP.y + y].CurUnit != null && !line.IsEvading) { break; }

                            //Debug.Log("     " + (unitP.x + x) + " " + (unitP.y + y) + " cur unit- " + BoardManager.Instance.Board[unitP.x + x].Cells[unitP.y + y].CurUnit);
                            lineS += line.Positions[i] + unitP + " ";
                            iPositions.Add(line.Positions[i] + unitP);
                        }

                        //For some reason sometimes the code can decide to select positions what the unit shouldn't physically be able to fit in, which leads to problems, so this check fixes that
                        // this issue will probbly come up sooner but fuck it I guess. Future me I hope this smug comment was worth your patience 
                        if (iPositions.Count == BoardManager.Instance.Get_UnitPositions(unit).Count) { res.Add(iPositions); }            
                        //Debug.Log(i + ": " + lineS);
                    }           
            }   
            #endregion
            return res;
        }

    //Returns all possible units what a source (unit) can affect using their current attack Zones
    public List<Unit> GetPossibleTargets(Unit source, List<Unit.Keyword> keywords)
    {
        // Debug.Log("Possible movement positions:");
        List<Unit> res = new List<Unit>();

        #region This goes through each individaul line and stops if it encounters a unit
        foreach (CellGuide.Line line in source.CurAttackZone.Lines)
        {
            for (int i = 0; i < line.Positions.Count; i++)
            {
                foreach (Vector2Int unitP in BoardManager.Instance.Get_UnitPositions(source))
                {
                    if (!BoardManager.Instance.IsInBounds(line.Positions[i] + unitP)) { break; }
                    int x = line.Positions[i].x; int y = line.Positions[i].y;
                    if (BoardManager.Instance.Board[unitP.x + x].Cells[unitP.y + y].CurUnit != null && !line.IsEvading) 
                    { 
                        Unit curUn = BoardManager.Instance.Board[unitP.x + x].Cells[unitP.y + y].CurUnit;
                        if (!res.Contains(curUn) && UnitHasAllKeywords(curUn, keywords)) { res.Add(curUn); }
                        break;
                    }
                    
                }

            }
        }
        #endregion
        return res;

    }
    //returns true if unit has all of the keywords in "keywords"
    public bool UnitHasAllKeywords(Unit u, List<Unit.Keyword> keywords)
    {
        foreach (var k in keywords) { if (!u.Keywords.Contains(k)) { return false; } }
        return true;
    }

    //Groups units from a list into a different list only if they have the keywords in "keywords"
    List<Unit> GroupUnitsByKeywords(List<Unit> all, List<Unit.Keyword> keywords)
    {
        List<Unit> cycled = new List<Unit>();
        foreach (var v in all)
        {
            bool allContain = UnitHasAllKeywords(v, keywords);

            if (allContain) { cycled.Add(v); }
        }
        return cycled;
    }
    
    //This is called by click events on buttons
    public void CellClickHandle(Vector2Int coords)
    {
        //Debug.Log("SOMEONE CLICKED THE CELL ON " + coords);
        StartCoroutine(CellClickCoroutine(coords));
    }
    
    //Makes different things happen when players clicks on a board cell depending on how exactly player presses a cell (For example, moving a unit, selecting a unit, etc...)
    IEnumerator CellClickCoroutine(Vector2Int coords)
    {
        yield return new WaitForSeconds(0.001f); //For some reason this is vital, otherwise Unity shits itself trying to assign and end a Coroutine at the same time 

        if (CurUnitSelected != null && BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit == null) //If have a unit and cell is unoccupied - a)move it to the cell, otherwise - b)select a unit on that cell
        { //a)

            List<Vector2Int> nCoords = new List<Vector2Int>(); nCoords.Add(coords);

            List<Unit> unitToList = new List<Unit>();unitToList.Add(CurUnitSelected);
            yield return StartCoroutine(Action(AcionType.Move, unitToList, null, nCoords));
            CurUnitSelected = null;

        }
        else
        { //b)
            if (BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit != null) //Check whenever there is a unit on a clicked cell
            {
                List<Vector2Int> nCoords = new List<Vector2Int>(); nCoords.Add(coords);

                yield return StartCoroutine(Action(AcionType.Select, null, null, nCoords));
            }
            else
            {
                Debug.Log("HAVE NOTHING SELECTED, " + coords + " HAS NO UNITS ");
                //No unit on that cell, do nothing
            }
        }
        yield return null;
    }

    //Continuously cycles through each action in current action queue 
    IEnumerator GoThroughActions()
    {
        yield return new WaitForSeconds(0.0001f);

        while (ActionQueue.Count > 0)
        {
            yield return StartCoroutine(ActionQueue.Dequeue().IEnum);
            yield return new WaitForSeconds(0.0001f);
        }
        ActionQueue.Clear();
        GoingThroughActions = null;
    }
    private void FixedUpdate()
    {
        while (ActionQueue.Count > 0 && GoingThroughActions == null)
        {
            GoingThroughActions = StartCoroutine(GoThroughActions());
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown("q"))
        {
            Debug.Log("CURRENT ACTION QUEUE:");
            foreach (var a in ActionQueue) { Debug.Log(a.Name); }

        }
        if (Input.GetKeyDown("a"))
        {
            Debug.Log("PRESSED THE ATTACK BUTTON");
            List<Unit.Keyword> k = new List<Unit.Keyword>();k.Add(Unit.Keyword.Player);
            StartCoroutine(Action(AcionType.KeywordedAttack, null, k, null));
        }
    }
}
