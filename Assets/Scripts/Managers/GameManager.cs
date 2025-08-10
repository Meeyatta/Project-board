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
        GlobalScore() - Makes it so all objects with Score ability add points to either the enemy or the player
       
        --ABILITIES--

        Score(ActionTargetUnits) - Ability: Ability_Score, checks for units nearby, adds points to player if more player units, 
            adds points to enemy if more enemy units
        Slip(ActionTargetUnits) - Ability: Ability_Slippery, Moves unit into a random direction

        --ITEMS--

        Item_WaterBucket - Covers a 3x3 square in Water
        Item_OilBucket - Covers a 3x3 square in Oil
*/

/*
    LIST OF USEFUL FUNCTIONS:
        void Cancel(InputAction.CallbackContext context) - fires an event if the player cancels an action. 
            Appropriate cancel function should be applied by currently active action
        bool UnitHasAllKeywords(Unit u, List<Keyword> keywords) - returns true if unit has all of the keywords in "keywords"
        List<Unit> Get_OnlyUnitsWithKeywords(List<Unit> all, List<Keyword> keywords) - Groups units from a list into a different list only if
            they have the keywords in "keywords"

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

    public bool ShouldDebug;

    [HideInInspector] public UnityEvent CancelEvent;
    public enum ActionType 
    { 
    Attack, AttackFromKeyworded, Move, Place, SelectUnit, PlayerCreate, Pre_NextTurn, NextTurn, Create, CreateBattlefield, Deploy, Redeploy,
    DeployNew, DeployPlayerStarters, GlobalScore,

    Score, Slip,

    WaterBucket, OilBucket,
    };
    [HideInInspector] public Unit CurUnitSelected = null; //What unit is currently selected, if no unit - should be null
    public static GameManager Instance;
    public Coroutine C_GoingThroughActions;
    public Coroutine C_UnitSelect;
    public IEnumerator I_PositionSelect;
    public ActionSlot CurrentAction;
    public Queue<ActionSlot> ActionQueue = new Queue<ActionSlot>();

    #region Events 
    public UnityEvent E_Restart;

    public UnityEvent<List<Unit>> E_ShowMovement;
    public UnityEvent<List<Unit>> E_HideMovement;

    public UnityEvent<List<Unit>> E_ShowDeployment;
    public UnityEvent E_HideDeployment;

    public UnityEvent<List<Unit>> E_ShowPlacement;
    public UnityEvent<List<Unit>> E_HidePlacement;
    void Start()
    {
        Action_NextTurn.E_Turn_Functional.AddListener(UnselectCurrentUnit);
    }
    void OnDisable()
    {
        Action_NextTurn.E_Turn_Functional.RemoveListener(UnselectCurrentUnit);

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
        if (ShouldDebug) Debug.Log("GameManager Action");
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
                C_UnitSelect = StartCoroutine(Action_SelectUnit.Select(parameters));
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
                if (ShouldDebug) Debug.Log("Next turn action");
                ActionSlot nextturn = new ActionSlot(Action_NextTurn.NextTurn(parameters), ActionType.NextTurn, parameters);
                ActionQueue.Enqueue(nextturn);
                break;
            #endregion NextTurn()

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
            //DEPRECATED, USE COROUTINES
            #region CreateBattlefield()
            case ActionType.CreateBattlefield:
                //ActionSlot createBattlefield = new ActionSlot(Action_CreateBattlefield.CreateBattlefield(parameters), ActionType.CreateBattlefield, parameters);
                //ActionQueue.Enqueue(createBattlefield);

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

            //Makes it so all objects with Score ability add points to either the enemy or the player
            #region GlobalScore()
            case ActionType.GlobalScore:
                ActionSlot globalScore = new ActionSlot(Action_GlobalScore.GlobalScore(parameters), ActionType.GlobalScore, parameters);
                ActionQueue.Enqueue(globalScore);

                break;
            #endregion GlobalScore()
         
            // --Items--

            //Water Bucket - covers a selected 3x3 square in Water
            #region WaterBucket()
            case ActionType.WaterBucket:
                ActionSlot water = new ActionSlot(Item_WaterBucket.WaterBucket(parameters), ActionType.WaterBucket, parameters);
                ActionQueue.Enqueue(water);

                break;
            #endregion

            //Oil Bucket - covers a selected 3x3 square in Oil
            #region OilBucket()
            case ActionType.OilBucket:
                ActionSlot oil = new ActionSlot(Item_OilBucket.OilBucket(parameters), ActionType.OilBucket, parameters);
                ActionQueue.Enqueue(oil);

                break;
            #endregion

            //Means I forgot to make an action for this type
            #region Default(...)
            default:
                Debug.LogError("Action not written: " + parameters.Type);
                break;
            #endregion Default(...)
        }
        yield return null;
    }
    public void UnselectCurrentUnit(Side s)
    {
        CurUnitSelected = null;
    }

    bool CancelCond()
    {
        if (CurrentAction != null && CurrentAction.Type == ActionType.DeployPlayerStarters) return false;

        return true;
    }
    //Fires an event if the player cancels an action. Appropriate cancel function should be applied by currently active action
    public void Cancel(InputAction.CallbackContext context)
    {
        if (context.performed && CancelCond())
        {            
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

    //Removes action from the queue/current action because it is done/canceled
    public void RemoveAction(ActionParameters parms)
    {
        if (CurrentAction == null) { return; }

        if (CurrentAction.Params == parms) { CurrentAction = null; }

        if (ActionQueue != null && ActionQueue.Count > 0 &&
            ActionQueue.Peek().Params == parms) { ActionQueue.Dequeue(); } // <- Might need to change it to "while", but I am afraid Unity will destroy itself thinking its an infinite loop
    }

    //Continuously cycles through each action in current action queue 
    public bool SwitchedToAnotherAction = false; //This is so "click check" can stop checking cell clicks while we change from one action to another
    public bool SwitchedToNoAction = false; //This is so "click check" can stop checking cell clicks while we change from an action to none
    IEnumerator GoThroughActions()
    {
        yield return new WaitForSeconds(Time.fixedDeltaTime / 100);

        while (ActionQueue.Count > 0)
        {
            if (CurrentAction != null) { yield return new WaitForSeconds(Time.fixedDeltaTime); continue; }

            CurrentAction = ActionQueue.Dequeue();

            //Debug.Log("Switched to another action");
            SwitchedToAnotherAction = true;
            yield return StartCoroutine(CurrentAction.IEnum);
            yield return new WaitForSeconds(Time.fixedDeltaTime);
            SwitchedToAnotherAction = false;
        }
        ActionQueue.Clear();
        CurrentAction = null;
        C_GoingThroughActions = null;

        //Debug.Log("Switched to no action");
        SwitchedToNoAction = true;
        yield return new WaitForSeconds(Time.fixedDeltaTime);
        SwitchedToNoAction = false;
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
        #region Debug inputs
        //if (Input.GetKeyDown("s"))
        //{
        //    ActionParameters parameters = new ActionParameters(ActionType.NextTurn, null, null, null, null, 0);
        //    StartCoroutine(Action(parameters));
        //}

        //Debug.Log("-----");
        //if (CurrentAction != null) Debug.Log(")" + CurrentAction.Type);
        //if (ActionQueue != null && ActionQueue.Count > 0) Debug.Log(")" + ActionQueue.Peek().Type);
        //Debug.Log("-----");

        //if (Input.GetKeyDown("q"))
        //{
        //    Debug.Log("CURRENT ACTION QUEUE:");
        //    if (CurrentAction != null) Debug.Log(")" + CurrentAction);
        //    if (ActionQueue != null && ActionQueue.Count > 0) Debug.Log(")" + ActionQueue.Peek().Type);
        //}
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

        }
}
