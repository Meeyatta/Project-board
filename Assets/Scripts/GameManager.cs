using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
        public ActionType Type;

        public ActionSlot(IEnumerator e, ActionType t)
        {
            IEnum = e;
            Type = t;
        }
    }
    public GameObject TESTunittocreate;
    public UnityEvent<List<Vector2Int>> ClickBackEvent;
    public enum ActionType { Attack, KeywordedAttack, Move, ForcedMove, SelectUnit, PlayerCreate };
    public Unit CurUnitSelected; //What unit is currently selected, if no unit - should be null
    public static GameManager Instance;
    public Coroutine C_GoingThroughActions;
    public Coroutine C_UnitSelect;
    public IEnumerator I_PositionSelect;
    public ActionSlot CurrentAction;
    public Queue<ActionSlot> ActionQueue = new Queue<ActionSlot>();
    
    #region Events 
    public UnityEvent<List<Unit>> ShowMovementEvent;
    public UnityEvent<List<Unit>> HideMovementEvent;

    public UnityEvent<List<Unit>> ShowPlacementEvent;
    public UnityEvent<List<Unit>> HidePlacementEvent;
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

    public void ActionWrapper(ActionParameters parameters)
    {
        StartCoroutine( Action(parameters) );
    }
    //Can be called by a variety of things to initiate a variety of actions. PLEASE REMEMBER TO PUT IN BRACKETS WHAT INFO IS NEEDED TO DO THE ACTION
        /*TODO : Separate this function into several smaller ones with different required parameters, since currently the function has a lot of parametres used only by a singluar type of action,
        which is messy and needs a bunch of null statements */
    public IEnumerator Action(ActionParameters parameters)
    {
        switch (parameters.Type)
        {
            //Move the unit to the coordinates if it can move there with it's moveset, Requires: (ActionTargetUnit, CellsCoordinates)
            #region Move(ActionTargetUnits, CellsCoordinates)
            case ActionType.Move:
                ActionSlot move = new ActionSlot(Action_Move.Instance.Move(parameters.ActionTargetUnits[0], parameters.CellsCoordinates), ActionType.Move);
                ActionQueue.Enqueue(move);             
                break;
            #endregion Move(ActionTargetUnits, CellsCoordinates)

            //Set a unit under coordinates as a Current selected unit by the player, Requires: (CellsCoordinates)
            //  This one should be called with StartCoroutine instead of yield return, because unit can be selected while other actions are done
            #region Select(CellsCoordinates)
            case ActionType.SelectUnit:
                C_UnitSelect = StartCoroutine( Action_SelectUnit.Instance.Select(parameters.CellsCoordinates) );
                break;
            #endregion Select(CellCoordinates)

            //Move the unit to the coordinates, doesn't check for unit's moveset Requires: (ActionTargetUnit, CellsCoordinates)
            //TODO: Separate into a unique script, currently the corotuine in inside GameManager
            #region ForcedMove(ActionTargetUnits, CellsCoordinates)
            case ActionType.ForcedMove:
                ActionSlot forcedmove = new ActionSlot(ForcedMove(parameters.ActionTargetUnits[0], parameters.CellsCoordinates), ActionType.ForcedMove);
                ActionQueue.Enqueue(forcedmove);
                break;
            #endregion ForcedMove(ActionTargetUnits, CellsCoordinates)

            //Make a target unit initiate an attack on all units in it's attack zone
            #region Attack(ActionTargetUnits)
            case ActionType.Attack:
                ActionSlot attack = new ActionSlot(Action_Attack.Instance.Attack(parameters.ActionTargetUnits), ActionType.Attack);
                ActionQueue.Enqueue(attack);
                break;
            #endregion Attack(ActionTargetUnits)
                
            //Make all units with specific keywords initiate an attack on all units in their individual attack zones
            #region KeywordedAttack(Keywords)
            case ActionType.KeywordedAttack:
                ActionSlot keywordedattack = 
                    new ActionSlot(Action_Attack.Instance.Attack(
                        GroupUnitsByKeywords(BoardManager.Instance.Get_AllUnitsOnBoard(), parameters.Keywords)), ActionType.KeywordedAttack);

                ActionQueue.Enqueue(keywordedattack);
                break;
            #endregion KeywordedAttack(Keywords)

            //Create a specific unit on coordinates present on the board
            #region PlayerCreate(GameObject Object)
            case ActionType.PlayerCreate:
                ActionSlot playercreate = new ActionSlot(Action_PlayerCreate.Instance.PlayerCreate(parameters.Object), ActionType.PlayerCreate);
                ActionQueue.Enqueue(playercreate);

                break;
            #endregion Create(GameObject Object, Vector2Int CellsCoordinates)

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
            foreach (Moveset.Line line in unit.CurMoveset.Lines)
                {
                    bool isObscured = false;
                    for (int i = 0; i < line.Positions.Count; i++)
                    {
                        string lineS = "";
                        List<Vector2Int> iPositions = new List<Vector2Int>();
                        foreach (Vector2Int unitP in BoardManager.Instance.Get_UnitPositions(unit))
                        {                    
                            if (!BoardManager.Instance.IsInBounds(line.Positions[i] + unitP) || isObscured) { break; }
                            int x = line.Positions[i].x; int y = line.Positions[i].y;
                            if (BoardManager.Instance.Board[unitP.x + x].Cells[unitP.y + y].CurUnit != null) 
                            {
                                if (!line.IsEvading) { isObscured = true; }
                                break;
                            }
                            
                            //Debug.Log("     " + (unitP.x + x) + " " + (unitP.y + y) + " cur unit - " + BoardManager.Instance.Board[unitP.x + x].Cells[unitP.y + y].CurUnit);
                            lineS += line.Positions[i] + unitP + " ";
                            iPositions.Add(line.Positions[i] + unitP);                          
                        }

                        //For some reason sometimes the code can decide to select positions what the unit shouldn't physically be able to fit in,
                        //  which leads to problems, so this check fixes that.
                        //  This issue will probably come up later but fuck it I guess. Future me I hope this smug comment was worth your patience 
                        if (iPositions.Count == BoardManager.Instance.Get_UnitPositions(unit).Count && !isObscured) { res.Add(iPositions); }            
                        
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
        if (source.CurAttackZone.Lines.Count == 0) { return res; }

        foreach (var line in source.CurAttackZone.Lines)
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

        //If we are currently selecting a position for something - a1)   b1) work normally
        if (CurrentAction.Type == ActionType.PlayerCreate && Action_PlayerCreate.Instance.IsWaitingForData)
        { //a1
            Debug.Log(CurrentAction.Type);  //<- Important note, current action is stored in a separate field, not in the queue
            List<Vector2Int> nCoords = new List<Vector2Int>(); nCoords.Add(coords);
            ClickBackEvent.Invoke(nCoords);
        }
        else
        { //b1
            //If have a unit and cell is unoccupied - a)move it to the cell, otherwise - b)select a unit on that cell
            if (CurUnitSelected != null && BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit == null)
            { //a)

                List<Vector2Int> nCoords = new List<Vector2Int>(); nCoords.Add(coords);

                List<Unit> unitToList = new List<Unit>(); unitToList.Add(CurUnitSelected);

                ActionParameters parameters = new ActionParameters(ActionType.Move, unitToList, null, nCoords, null);
                yield return StartCoroutine(Action(parameters));
                CurUnitSelected = null;

            }
            else
            { //b)
                if (BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit != null) //Check whenever there is a unit on a clicked cell
                {
                    List<Vector2Int> nCoords = new List<Vector2Int>(); nCoords.Add(coords);

                    //If the selected unit is a player unit - aa) select it, otherwise - bb) TODO:
                    if (BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit.Keywords.Contains(Unit.Keyword.Player))
                    { //aa)
                        ActionParameters parameters = new ActionParameters(ActionType.SelectUnit, null, null, nCoords, null);
                        yield return StartCoroutine(Action(parameters));
                    }
                    else
                    {
                        yield return new WaitForSeconds(0.1f);
                    }

                }
                else
                {
                    Debug.Log("HAVE NOTHING SELECTED, " + coords + " HAS NO UNITS ");
                    //No unit on that cell, do nothing
                }
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
            CurrentAction = ActionQueue.Dequeue();
            yield return StartCoroutine(CurrentAction.IEnum);
            yield return new WaitForSeconds(0.0001f);
        }
        ActionQueue.Clear();
        CurrentAction = null;
        C_GoingThroughActions = null;
    }
    private void FixedUpdate()
    {
        while (ActionQueue.Count > 0 && C_GoingThroughActions == null)
        {
            C_GoingThroughActions = StartCoroutine(GoThroughActions());
        }
    }
    private void Update()
    {



        if (Input.GetKeyDown("q"))
        {
            Debug.Log("CURRENT ACTION QUEUE:");
            foreach (var a in ActionQueue) { Debug.Log(a.Type); }

        }
        if (Input.GetKeyDown("a"))
        {
            Debug.Log("PRESSED THE ATTACK BUTTON");
            List<Unit.Keyword> k = new List<Unit.Keyword>();k.Add(Unit.Keyword.Player);

            ActionParameters parameters = new ActionParameters(ActionType.KeywordedAttack, null, k, null, null);
            StartCoroutine(Action(parameters));
        }
    }
}
