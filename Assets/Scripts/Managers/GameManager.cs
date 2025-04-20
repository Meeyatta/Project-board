using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

//Main script, handles all of the actions what can be done with units

/*
    All main functions in action specific scripts MUST have:
        ActionParameters parameters - Parameters used by whatever needs us to performs a function. This also holds an identifier for this action   
    If an action can be canceled at some point, it should have a function for that assigned to Gamemanager's "CancelEvent" 
        (Invoked in Cancel(InputAction.CallbackContext context))

    LIST OF ACTIONS:
        Move(ActionTargetUnits, CellsCoordinates) - Move the unit to the coordinates if it can move there with it's moveset, 
        Select(CellsCoordinates) - Set a unit under coordinates as a Current selected unit by the player
        ForcedMove(ActionTargetUnits, CellsCoordinates) - Move the unit to the coordinates, doesn't check for unit's moveset 

        Attack(ActionTargetUnits) - Make a target unit initiate an attack on all units in it's attack zone
        KeywordedAttack(Keywords) - Make all units with specific keywords initiate an attack on all units in their individual attack zones

        PlayerCreate(Object) - Awaits for player's input on cell coordinates, then places the unit on these coordinates
        Action_NextTurn() - Makes all units on one side attack, then scores all objectives, then passes the turn/round to the other side
        Create(GameObject Object, List<Vector2Int> CellsCoordinates, int IntNumber) - Creates a new unit on coordinates on one of the sides (0 - palyer, 1 - enemy)
          
        CreateBattlefield(TODO: More parameters) - Places the battlefield things like objectives & obstacles
        Deploy(List<Unit> roster, int amount, List<Vector2Int> dZone) - Deploys <amount> number of units from <roster> within <dZone> coordinates
        Redeploy(Unit unit, List<Vector2Int> CellsCoordinates) - Places the unit on the position within the deployment zone
        DeployNew() - Gives player a choice between 3 not-deployed units and places one of them on the board
        DeployPlayerStarter(int IntNumber) - Places makes player draw one of 3 units a certain amount of times
       
        --ABILITIES--

        Score(ActionTargetUnits) - Ability: Ability_Score, checks for units nearby, adds points to player if more player units, 
            adds points to enemy if more enemy units
        Slip(ActionTargetUnits) - Ability: Ability_Slippery, Moves unit into a random direction
*/

/*
    LIST OF USEFUL FUNCTIONS:
        void Cancel(InputAction.CallbackContext context) - fires an event if the player cancels an action. 
            Appropriate cancel function should be applied by currently active action
        bool UnitHasAllKeywords(Unit u, List<Keyword> keywords) - returns true if unit has all of the keywords in "keywords"
        List<Unit> Get_OnlyUnitsWithKeywords(List<Unit> all, List<Keyword> keywords) - Groups units from a list into a different list only if
            they have the keywords in "keywords"
        void ResetMovement(List<Unit.keyword> keywords) - resets all units with keywords in a list to be able to move again. 

        There are 2 ways game checks for pressing on cells:
            1) Each cell is a button what sends an event call with it's position when it is pressed
            2) "void CheckUnitUnderCursor()" in Update continuously checks if we aim at a unit and calls an event when we click
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
    
    public UnityEvent<Vector2Int> ClickBackEvent;
    [HideInInspector] public UnityEvent CancelEvent;
    public enum ActionType 
    { 
    Attack, AttackFromKeyworded, Move, Place, SelectUnit, PlayerCreate, Pre_NextTurn, NextTurn, Create, CreateBattlefield, Deploy, Redeploy,
    DeployNew, DeployPlayerStarters,

    Score, Slip,
    };
    public LayerMask LayerMask;
    [HideInInspector] public Unit CurUnitSelected = null; //What unit is currently selected, if no unit - should be null
    public static GameManager Instance;
    public Coroutine C_GoingThroughActions;
    public Coroutine C_UnitSelect;
    public IEnumerator I_PositionSelect;
    public ActionSlot CurrentAction;
    public Queue<ActionSlot> ActionQueue = new Queue<ActionSlot>();
    
    #region Events 
    public UnityEvent<List<Unit>> ShowMovementEvent;
    public UnityEvent<List<Unit>> HideMovementEvent;

    public UnityEvent<List<Unit>> ShowDeploymentEvent;
    public UnityEvent HideDeploymentEvent;

    public UnityEvent<List<Unit>> ShowPlacementEvent;
    public UnityEvent<List<Unit>> HidePlacementEvent;
    void Start()
    {
        ScoreManager.Instance.eTurnEvent_Functional.AddListener(UnselectCurrentUnit);
    }
    void OnDisable()
    {
        ScoreManager.Instance.eTurnEvent_Functional.RemoveListener(UnselectCurrentUnit);

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
        
        If it's possible for an action to interact with abilities, it should call these abilities in script
    */
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
            #endregion PlayerCreate(GameObject Object)

            //Makes all units on one side attack, then scores all objectives, then passes the turn/round to the other side
            #region NextTurn()
            case ActionType.NextTurn:
                ActionSlot prenextturn = new ActionSlot(Action_NextTurn.Pre_nextTurn(parameters), ActionType.Pre_NextTurn, parameters);
                ActionQueue.Enqueue(prenextturn);

                yield return new WaitForSeconds(0.3f); // <- Load-bearing delay, don't let it get shorter than 0.3 seconds

                ActionSlot nextturn = new ActionSlot(Action_NextTurn.NextTurn(parameters), ActionType.NextTurn, parameters);
                ActionQueue.Enqueue(nextturn);

                break;
            #endregion Create(GameObject Object, Vector2Int CellsCoordinates)

            //Creates a new unit on coordinates
            #region Create(GameObject Object, List<Vector2Int> CellsCoordinates)
            case ActionType.Create:
                ActionSlot create = new ActionSlot(Action_Create.Create(parameters), ActionType.Create, parameters);
                ActionQueue.Enqueue(create);

                break;
            #endregion Create(GameObject Object, Vector2Int CellsCoordinates)

            //Places randomly a number of units from roster in their deployment zone
            #region Deploy(List<Vector2Int> CellsCoordinates, GameObject Object, int IntNumber)
            case ActionType.Deploy:
                ActionSlot deployment = new ActionSlot(Action_Deployment.Deploy(parameters), ActionType.Deploy, parameters);
                ActionQueue.Enqueue(deployment);

                break;
            #endregion Deploy(List<Vector2Int> CellsCoordinates, GameObject Object, int IntNumber)

            //Places the battlefield things like objectives & obstacles
            #region CreateBattlefield()
            case ActionType.CreateBattlefield:
                ActionSlot createBattlefield = new ActionSlot(Action_CreateBattlefield.CreateBattlefield(parameters), ActionType.CreateBattlefield, parameters);
                ActionQueue.Enqueue(createBattlefield);

                break;
            #endregion CreateBattlefield()

            //Places the unit on the position within the deployment zone
            #region Redeploy(Unit unit, List<Vector2Int> CellsCoordinates)
            case ActionType.Redeploy:
                ActionSlot redeploy = new ActionSlot(Action_Redeploy.Redeploy(parameters), ActionType.Redeploy, parameters);
                ActionQueue.Enqueue(redeploy);

                break;
            #endregion Redeploy()

            //Gives player a choice between 3 not-deployed units and places one of them on the board
            #region DeployNew()
            case ActionType.DeployNew:
                ActionSlot deployNew = new ActionSlot(Action_DeployNewPlayerUnit.DeployNewUnit(parameters), ActionType.DeployNew, parameters);
                ActionQueue.Enqueue(deployNew);

                break;
            #endregion DeployNew()

            //Places makes player draw one of 3 units a certain amount of times
            #region DeployPlayerStarter(int IntNumber)
            case ActionType.DeployPlayerStarters:
                ActionSlot starterDeployment = new ActionSlot(Action_DeployPlayerStarters.DeployPlayerStarters(parameters), ActionType.DeployPlayerStarters, parameters);
                ActionQueue.Enqueue(starterDeployment);

                break;
            #endregion DeployPlayerStarter(int IntNumber)

            /* --ABILITIES-- */

            //Goes through every objective and scores for the player or enemy (Depends on what is passed in parameters)
            #region Score()
            case ActionType.Score:

                ActionSlot score = new ActionSlot(Ability_Score.GlobalScore(parameters), ActionType.Score, parameters);
                ActionQueue.Enqueue(score);

                break;
            #endregion Create(GameObject Object, Vector2Int CellsCoordinates)

            //Moves unit into a random direction
            #region Slip(ActionTargetUnits)
            case ActionType.Slip:

                ActionSlot slip = new ActionSlot(Ability_Slippery.Slip(parameters), ActionType.Slip, parameters);
                ActionQueue.Enqueue(slip);

                break;
            #endregion Slip(ActionTargetUnits)



            //Means I forgot to make an action for this type
            #region Default(...)
            default:
                Debug.LogError("Action not written");
                break;
            #endregion Default(...)
        }
        yield return null;
    }
    public void UnselectCurrentUnit(ScoreManager.Side s)
    {
        CurUnitSelected = null;
    }
    //Fires an event if the player cancels an action. Appropriate cancel function should be applied by currently active action
    public void Cancel(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Cancel");
            CancelEvent.Invoke();
        }
    }

    //returns true if unit has all of the keywords in "keywords"
    public bool UnitHasAllKeywords(Unit u, List<Keyword> keywords)
    {
        foreach (var k in keywords) { if (!u.CurKeywords.Contains(k)) { return false; } }
        return true;
    }

    //Returns units from the list only if they have the keywords in "keywords"
    public List<Unit> Get_OnlyUnitsWithKeywords(List<Unit> all, List<Keyword> keywords)
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



    public IEnumerator TutorialClickCoroutine(Vector2Int coords)
    {
        yield return new WaitForSeconds(Time.deltaTime * 0.01f); //For some reason this is vital, otherwise Unity shits itself

        #region Are we selecting a position for creating a unit?
        if (Action_PlayerCreate.IsWaitingForData && Action_PlayerCreate.CanCreateThere(Action_PlayerCreate.Unit, coords))
        #region Yes - Invoke an event to send coordinates where the unit is going to be created (If can be created)
        { //a1
            //Debug.Log("Creating a unit"); 
            ClickBackEvent.Invoke(coords);
        }
        #endregion

        #region No - Check if there is a unit to move to these coordinates
        else
        {
            #region Do we have a unit and cell is unoccupied?
            if (CurUnitSelected != null && BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit == null &&
                ScoreManager.Instance.PlayerTurnActionCondition())
            #region Yes - Move the unit
            {
               List<Vector2Int> nCoords = new List<Vector2Int>(); nCoords.Add(coords);
                    List<Unit> unitToList = new List<Unit>(); unitToList.Add(CurUnitSelected);

                    ActionParameters parameters = new ActionParameters(ActionType.Move, unitToList, null, nCoords, null, 0);
                    yield return StartCoroutine(Action(parameters));
                    CurUnitSelected = null;
                
                #endregion
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
                        && CurUnitSelected != BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit)
                    #region Yes - select it if we can
                    {
                        ActionParameters parameters = new ActionParameters(ActionType.SelectUnit, null, null, nCoords, null, 0); yield return StartCoroutine(Action(parameters)); 
                        //else { Debug.Log(ScoreManager.Instance.CurTurn); }
                    }
                    #endregion

                    #region No - TODO:
                    else
                    { //b4)
                        yield return new WaitForSeconds(0.01f);
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
        }
        #endregion


        yield return null;
    }

    /*
        Makes different things happen when players clicks on a board cell depending on how exactly player presses a cell
        (For example, moving a unit, selecting a unit, etc...)
    */
    IEnumerator CellClickCoroutine(Vector2Int coords)
    {
        yield return new WaitForSeconds(Time.deltaTime * 0.01f); //For some reason this is vital, otherwise Unity shits itself

        if (TutorialManager.Instance != null) { yield return TutorialClickCoroutine(coords); yield break; }

        #region Are we selecting a position for creating a unit?
        if (Action_PlayerCreate.IsWaitingForData && Action_PlayerCreate.CanCreateThere(Action_PlayerCreate.Unit, coords))
        #region Yes - Invoke an event to send coordinates where the unit is going to be created (If can be created)
        { //a1
            //Debug.Log("Creating a unit"); 
            ClickBackEvent.Invoke(coords);
        }
        #endregion

        #region No - Check if there is a unit to move to these coordinates
        else
        {
            #region Do we have a unit and cell is unoccupied?
            if (CurUnitSelected != null && BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit == null &&
                ScoreManager.Instance.PlayerTurnActionCondition())
            #region Yes - Are we in the deployment phase?
            {
                #region Yes - Redeploy unit to the coordinates
                if (ScoreManager.Instance.CurRound <= 0)
                {
                    List<Vector2Int> nCoords = new List<Vector2Int>(); nCoords.Add(coords);
                    List<Unit> unitToList = new List<Unit>(); unitToList.Add(CurUnitSelected);

                    ActionParameters parameters = new ActionParameters(ActionType.Redeploy, unitToList, null, nCoords, null, 0);
                    yield return StartCoroutine(Action(parameters));
                    CurUnitSelected = null;
                }
                #endregion
                #region No - Move unit to the position
                else
                {
                    List<Vector2Int> nCoords = new List<Vector2Int>(); nCoords.Add(coords);
                    List<Unit> unitToList = new List<Unit>(); unitToList.Add(CurUnitSelected);

                    ActionParameters parameters = new ActionParameters(ActionType.Move, unitToList, null, nCoords, null, 0);
                    yield return StartCoroutine(Action(parameters));
                    CurUnitSelected = null;
                }
                #endregion
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
                        && CurUnitSelected != BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit)
                    #region Yes - select it if we can
                    {
                        if (ScoreManager.Instance.CurRound != 0)   
                        { ActionParameters parameters = new ActionParameters(ActionType.SelectUnit, null, null, nCoords, null, 0); yield return StartCoroutine(Action(parameters)); }   
                        //else { Debug.Log(ScoreManager.Instance.CurTurn); }
                    }
                    #endregion

                    #region No - TODO:
                    else
                    { //b4)
                        yield return new WaitForSeconds(0.01f);
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
        }
        #endregion

        #endregion

        yield return null;
    }

    //Removes action from the queue/current action because it is done/canceled
    public void RemoveAction(ActionParameters parms)
    {
        if (CurrentAction == null) { return; }

        if (CurrentAction.Params == parms) { CurrentAction = null; }

        if (ActionQueue != null && ActionQueue.Count > 0 &&
            ActionQueue.Peek().Params == parms) { ActionQueue.Dequeue(); } // <- Might need to change it to "while", but I am afraid Unity will destroy itself thinking its an infinite loop
    }

    //Continuously cycles through each action in current action queue 
    IEnumerator GoThroughActions()
    {
        yield return new WaitForSeconds(Time.deltaTime);

        while (ActionQueue.Count > 0)
        {
            if (CurrentAction != null) { yield return new WaitForSeconds(Time.deltaTime); continue; }

            CurrentAction = ActionQueue.Dequeue();
            yield return StartCoroutine(CurrentAction.IEnum);
            yield return new WaitForSeconds(Time.deltaTime);
        }
        ActionQueue.Clear();
        CurrentAction = null;
        C_GoingThroughActions = null;
    }

    #region Checking if player is hovering over a unit, if they click - this counts as clicking on that unit's cell
    float ClickCooldown = 30; float nextClickTime = 1;

    Transform LastPointedAt = null; public Unit CurrentPointedAtUnit = new Unit();
    Vector2Int CellClickP = Vector2Int.zero; Vector2Int UnitClickP = Vector2Int.zero;
    Vector2Int NullV2 = new Vector2Int(-1,-1);

    bool ShouldCheckForClicks()
    {
        
        if (nextClickTime > Time.time) return false;
        if (CurrentAction != null && CurrentAction.Params.Type == ActionType.PlayerCreate) { return false; }

        return true;
    }

    void CheckUnitUnderCursor()
    {
        if (!ShouldCheckForClicks()) return;

        RaycastHit hit;
        Vector3 vect = Input.mousePosition;
        vect.z = 999999;
        Vector3 cPos = Camera.main.ScreenToWorldPoint(vect);
        Physics.Raycast(Camera.main.transform.position, cPos, out hit, Mathf.Infinity, LayerMask);

        
        //Debug.Log("Raycastin time " + hit.transform);

        #region If hit something
        bool foundSmth = false;
        if (hit.transform != null )
        {

            //Debug.Log(hit.transform.gameObject.name);

            #region If hit unit
            if (hit.transform.tag.ToLower() == "unit")
            {
                //Debug.Log(hit.transform.gameObject.name + " was hit with raycast");
                #region If keep pointing at the same unit
                if (hit.transform == LastPointedAt)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        //Debug.Log(hit.transform.gameObject.name + " was clicked on");
                        foundSmth = true;
                    }
                }
                #endregion
                #region If hover over a new unit
                else
                {
                    //Debug.Log(hit.transform.gameObject.name + " is new, making current");
                    LastPointedAt = hit.transform;
                    Unit u = LastPointedAt.GetComponent<Unit>();
                    CurrentPointedAtUnit = u;
                    if (u != null) 
                    { 
                        List<Vector2Int> p = BoardManager.Instance.Get_UnitPositions(u);
                        if (p != null && p.Count > 0 ) { CellClickP = NullV2; UnitClickP = p[0]; }            
                    }
                }
                #endregion
            }
            #endregion
            #region If hit a cell
            else if (hit.transform.tag.ToLower() == "cell")
            {
                #region If hover and click on an old cell
                if (hit.transform == LastPointedAt)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        foundSmth = true;
                    }
                }
                #endregion
                #region If hover over a new cell
                else
                {
                    LastPointedAt = hit.transform;
                    BoardCell b = LastPointedAt.GetComponent<BoardCell>();
                    if (b != null) { CellClickP = b.Coordinates; }
                }
                #endregion
            }
            #endregion
        }
        else
        {
        }

        #region Check what position did we click on and send the click event
        if (foundSmth && Input.GetMouseButtonDown(0)) 
        {
            //Debug.Log(foundSmth + " " + Input.GetMouseButtonDown(0));

            if (CellClickP != NullV2)
            {
                CellClickHandle(CellClickP);
                nextClickTime = Time.time + ClickCooldown * Time.deltaTime;
            }
            else if (UnitClickP != NullV2)
            {
                CellClickHandle(UnitClickP);
                nextClickTime = Time.time + ClickCooldown * Time.deltaTime;
            }
        }
        #endregion

        #endregion
    }
    #endregion

    private void FixedUpdate()
    {
        while (ActionQueue.Count > 0 && C_GoingThroughActions == null)
        {
            C_GoingThroughActions = StartCoroutine(GoThroughActions());
        }
    }

    private void Update()
    {
        #region Debug inputs
        //if (Input.GetKeyDown("s"))
        //{
        //    ActionParameters parameters = new ActionParameters(ActionType.NextTurn, null, null, null, null, 0);
        //    StartCoroutine(Action(parameters));
        //}

        //if (Input.GetKeyDown("q"))
        //{
        //    Debug.Log("CURRENT ACTION QUEUE:");
        //    Debug.Log(")" + CurrentAction); Debug.Log(")" + ActionQueue.Peek().Type);

        //}
        //if (Input.GetKeyDown("a"))
        //{
        //    Debug.Log("PRESSED THE ATTACK BUTTON");
        //    List<Keyword> k = new List<Keyword>();k.Add(Keyword.Player);

        //    ActionParameters parameters = new ActionParameters(ActionType.AttackFromKeyworded, null, k, null, null, 0);
        //    StartCoroutine(Action(parameters));
        //}

        //if (Input.GetKeyDown("x"))
        //{
        //    Debug.Log("SCORING FOR ENEMY:");
        //    List<Keyword> k = new List<Keyword> { Keyword.Enemy };
        //    ActionParameters parameters = new ActionParameters(ActionType.Score, null, k, null, null, 0);
        //    StartCoroutine(Action(parameters));

        //}
        #endregion

        CheckUnitUnderCursor();
    }
}
