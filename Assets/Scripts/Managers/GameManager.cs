using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

//Main script, handles all of the actions what can be done with units

/*
    All main functions in action specific scripts MUST have:
        ActionParameters parameters - Parameters used by whatever needs us to performs a function. This also holds an identifier for this action   
 
    LIST OF ACTIONS:
        Move(ActionTargetUnits, CellsCoordinates) - Move the unit to the coordinates if it can move there with it's moveset, Requires: (ActionTargetUnit, CellsCoordinates)
        Select(CellsCoordinates) - Set a unit under coordinates as a Current selected unit by the player, Requires: (CellsCoordinates) 
        ForcedMove(ActionTargetUnits, CellsCoordinates) - Move the unit to the coordinates, doesn't check for unit's moveset Requires: (ActionTargetUnit, CellsCoordinates)
        Attack(ActionTargetUnits) - Make a target unit initiate an attack on all units in it's attack zone
        KeywordedAttack(Keywords) - Make all units with specific keywords initiate an attack on all units in their individual attack zones
        PlayerCreate(Object) - Awaits for player's input on cell coordinates, then places the unit on these coordinates
        Action_NextTurn() - makes all units of one side attack, score objectives and then pass turn onto the next side. If turn is passed twice, next round starts

        A_Score(ActionTargetUnits) - Ability: A_Score, checks for units nearby, adds points to player if only player units, adds points to enem if only enemy units
*/

/*
    LIST OF USEFUL FUNCTIONS:
        bool UnitHasAllKeywords(Unit u, List<Unit.Keyword> keywords) - returns true if unit has all of the keywords in "keywords"
        List<Unit> Get_OnlyUnitsWithKeywords(List<Unit> all, List<Unit.Keyword> keywords) - Groups units from a list into a different list only if
            they have the keywords in "keywords"
        void ResetMovement(List<Unit.keyword> keywords) - resets all units with keywords in a list to be able to move again. 
*/

public class GameManager : MonoBehaviour
{
    public class ActionSlot
    {
        public IEnumerator IEnum;
        public ActionType Type;
        public ActionParameters Params;

        public ActionSlot(IEnumerator e, ActionType t, ActionParameters p)
        {
            IEnum = e;
            Type = t;
            Params = p;
        }
    }
    public GameObject TESTunittocreate;
    public UnityEvent<Vector2Int> ClickBackEvent;
    [HideInInspector] public UnityEvent CancelEvent;
    public enum ActionType { Attack, AttackFromKeyworded, Move, Place, SelectUnit, PlayerCreate, Score, NextTurn };
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
    /*
        Can be called by a variety of things to initiate a variety of actions. 
        PLEASE REMEMBER TO PUT IN BRACKETS WHAT INFO IS NEEDED TO DO THE ACTION
        /*TODO : Separate this function into several smaller ones with different required parameters, since currently the function has a 
        lot of parametres used only by a singluar type of action, which is messy and needs a bunch of null statements 
    */
    public void Cancel(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Cancel");
            CancelEvent.Invoke();
        }
    }
    public IEnumerator Action(ActionParameters parameters)
    {
        switch (parameters.Type)
        {
            //Move the unit to the coordinates if it can move there with it's moveset, Requires: (ActionTargetUnit, CellsCoordinates)
            #region Move(ActionTargetUnits, CellsCoordinates)
            case ActionType.Move:
                ActionSlot move = new ActionSlot(Action_Move.Move(parameters), ActionType.Move, parameters);
                ActionQueue.Enqueue(move);             
                break;
            #endregion Move(ActionTargetUnits, CellsCoordinates)

            //Set a unit under coordinates as a Current selected unit by the player, Requires: (CellsCoordinates)
            //  This one should be called with StartCoroutine instead of yield return, because unit can be selected while other actions are done
            #region Select(CellsCoordinates)
            case ActionType.SelectUnit:
                C_UnitSelect = StartCoroutine( Action_SelectUnit.Select(parameters));
                break;
            #endregion Select(CellCoordinates)

            //Move the unit to the coordinates, doesn't check for unit's moveset Requires: (ActionTargetUnit, CellsCoordinates)
            //TODO: Separate into a unique script, currently the corotuine in inside GameManager
            #region Place(ActionTargetUnits, CellsCoordinates)
            case ActionType.Place:
                ActionSlot forcedmove = new ActionSlot(Action_Place.Place(parameters), ActionType.Place, parameters);
                ActionQueue.Enqueue(forcedmove);
                break;
            #endregion ForcedMove(ActionTargetUnits, CellsCoordinates)

            //Make a target unit initiate an attack on all units in it's attack zone
            #region Attack(ActionTargetUnits)
            case ActionType.Attack:
                ActionSlot attack = new ActionSlot(Action_Attack.Attack(parameters), ActionType.Attack, parameters);
                ActionQueue.Enqueue(attack);
                break;
            #endregion Attack(ActionTargetUnits)
                
            //Make all units with specific keywords initiate an attack on all units in their individual attack zones
            #region KeywordedAttack(Keywords)
            case ActionType.AttackFromKeyworded:
                ActionSlot attackFromKeyword = new ActionSlot(Action_AttackFromKeyworded.AttackFromKeyworded(parameters), ActionType.AttackFromKeyworded, parameters);
                ActionQueue.Enqueue(attackFromKeyword);

                break;
            #endregion KeywordedAttack(Keywords)

            //Awaits for player's input on cell coordinates, then places the unit on these coordinates
            #region PlayerCreate(GameObject Object)
            case ActionType.PlayerCreate:
                ActionSlot playercreate = new ActionSlot(Action_PlayerCreate.PlayerCreate(parameters), ActionType.PlayerCreate, parameters);
                ActionQueue.Enqueue(playercreate);

                break;
            #endregion Create(GameObject Object, Vector2Int CellsCoordinates)

            /* --ABILITIES-- */

            //Goes through every objective and scores for the player or enemy (Depends on what is passed in parameters)
            #region Score()
            case ActionType.Score:

                ActionSlot score = new ActionSlot(Ability_Score.GlobalScore(parameters), ActionType.Score, parameters);
                ActionQueue.Enqueue(score);

                break;
            #endregion Create(GameObject Object, Vector2Int CellsCoordinates)

            #region NextTurn()
            case ActionType.NextTurn:
                ActionSlot prenextturn = new ActionSlot(Action_NextTurn.Pre_nextTurn(parameters), ActionType.NextTurn, parameters);
                ActionQueue.Enqueue(prenextturn);

                yield return new WaitForSeconds(0.2f);

                ActionSlot nextturn = new ActionSlot(Action_NextTurn.NextTurn(parameters), ActionType.NextTurn, parameters);
                ActionQueue.Enqueue(nextturn);

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
  
    //returns true if unit has all of the keywords in "keywords"
    public bool UnitHasAllKeywords(Unit u, List<Unit.Keyword> keywords)
    {
        foreach (var k in keywords) { if (!u.CurKeywords.Contains(k)) { return false; } }
        return true;
    }

    //Returns units from the list only if they have the keywords in "keywords"
    public List<Unit> Get_OnlyUnitsWithKeywords(List<Unit> all, List<Unit.Keyword> keywords)
    {
        List<Unit> cycled = new List<Unit>();
        foreach (var v in all)
        {
            bool allContain = UnitHasAllKeywords(v, keywords);

            if (allContain) { cycled.Add(v); }
        }
        return cycled;
    }

    //Resets all units with keywords in a list to be able to move again. This fucntion is used by "RoundEvent" in ScoreManager
    public void ResetMovement()
    {
        List<Unit> all = BoardManager.Instance.Get_AllUnitsOnBoard();
        foreach (var u in all) { u.Moved = false; }     
    }
    //This is called by click events on buttons
    public void CellClickHandle(Vector2Int coords)
    {
        //Debug.Log("SOMEONE CLICKED THE CELL ON " + coords);
        StartCoroutine(CellClickCoroutine(coords));
    }
    
    /*
        Makes different things happen when players clicks on a board cell depending on how exactly player presses a cell
        (For example, moving a unit, selecting a unit, etc...)
    */
    IEnumerator CellClickCoroutine(Vector2Int coords)
    {
        yield return new WaitForSeconds(0.001f); //For some reason this is vital, otherwise Unity shits itself trying to assign and end a Coroutine at the same time 

        #region If selecting a position for creating a unit - a1) Invoke an event to send coordinates   b1) check if can move a unit
        if (CurrentAction != null && CurrentAction.Type == ActionType.PlayerCreate && Action_PlayerCreate.IsWaitingForData)
        { //a1
            Debug.Log(CurrentAction.Type);  //<- Important note, current action is stored in a separate field, not in the queue
            ClickBackEvent.Invoke(coords);
        }
        else
        { //b1
            #region If have a unit and cell is unoccupied - a) move it to the cell, otherwise - b) check if there is a unit on that cell
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
            //If cell has a unit: a3)Check if it's a player unit   b3)Do nothing, clicked on an empty cell
                if (BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit != null) //Check whenever there is a unit on a clicked cell
                { //a3)
                    List<Vector2Int> nCoords = new List<Vector2Int>(); nCoords.Add(coords);

                    # region If the selected unit is a player unit - a4) select it, otherwise - b4) TODO:
                    if (BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit.CurKeywords.Contains(Unit.Keyword.Player))
                    { //a4)
                        ActionParameters parameters = new ActionParameters(ActionType.SelectUnit, null, null, nCoords, null);
                        yield return StartCoroutine(Action(parameters));
                    }
                    else
                    { //b4)
                        yield return new WaitForSeconds(0.1f);
                    }
                    #endregion If the selected unit is a player unit - a4) select it, otherwise - b4) TODO:

                }
                else
                { //b3)
                    Debug.Log("HAVE NOTHING SELECTED, " + coords + " HAS NO UNITS ");
                    //No unit on that cell, do nothing
                }
            }
            #endregion
        }

#endregion
        yield return null;
    }

    //Removes action from the queue/current action because it is done/canceled
    public void RemoveAction(ActionParameters parms)
    {
        if (CurrentAction == null) { return; }

        if (CurrentAction.Params == parms) { CurrentAction = null; }

        if (ActionQueue != null && ActionQueue.Count > 0 &&
            ActionQueue.Peek().Params == parms) { ActionQueue.Dequeue(); } // <- Might need to change it to "while", but I am afraid Unity will shit itself thinking its an infinite loop
    }

    //Continuously cycles through each action in current action queue 
    IEnumerator GoThroughActions()
    {
        yield return new WaitForSeconds(0.0001f);

        while (ActionQueue.Count > 0)
        {
            if (CurrentAction != null) { yield return new WaitForSeconds(1f); continue; }

            CurrentAction = ActionQueue.Dequeue();
            yield return StartCoroutine(CurrentAction.IEnum);
            yield return new WaitForSeconds(1f);
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
        if (Input.GetKeyDown("s"))
        {
            ActionParameters parameters = new ActionParameters(ActionType.NextTurn, null, null, null, null);
            StartCoroutine(Action(parameters));
        }

        if (Input.GetKeyDown("q"))
        {
            Debug.Log("CURRENT ACTION QUEUE:");
            foreach (var a in ActionQueue) { Debug.Log(")" + a.Type); }

        }
        if (Input.GetKeyDown("a"))
        {
            Debug.Log("PRESSED THE ATTACK BUTTON");
            List<Unit.Keyword> k = new List<Unit.Keyword>();k.Add(Unit.Keyword.Player);

            ActionParameters parameters = new ActionParameters(ActionType.AttackFromKeyworded, null, k, null, null);
            StartCoroutine(Action(parameters));
        }
        
        if (Input.GetKeyDown("x"))
        {
            Debug.Log("SCORING FOR ENEMY:");
            List<Unit.Keyword> k = new List<Unit.Keyword> { Unit.Keyword.Enemy };
            ActionParameters parameters = new ActionParameters(ActionType.Score, null, k, null, null);
            StartCoroutine(Action(parameters));

        }

        #region Constantly printing current action
        string ActionInfo = "";
        if (CurrentAction != null)
        {
            ActionInfo = CurrentAction.Type.ToString();
            //Debug.Log("Current action " + ActionInfo);

        }
        #endregion

    }
}
