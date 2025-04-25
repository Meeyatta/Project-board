using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

//Greet the player 
//Explain how to deploy unit at the start
//Explain how to pass the turn

//Explain how to move the unit
//Show how objectives look
//Explain how to capture objectives
//Explain how units attack
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    public bool CanClickOnCells = true;
    public bool Tutorial_CanPass;
    public float Tutorial_SupposedRoundTurn; //This keeps track of current roundturn (1 - first player round, 1.5 - first enemy round, etc...) 

    public float AestheticDelay;
    public float DelayBeforeAutomaticProceeding; float curDelay; //These are safeguards in case some parts of the tutorial loop infinitely
    [Header("First - deploying a unit")]
    public List<DialogueLine> FirstLines;
    [Header("Second - passing a turn")]
    public List<DialogueLine> SecondLines;  
    [Header("Third - moving")]
    public List<DialogueLine> ThirdLines;
    [Header("Fourth - objective scoring")]
    public List<DialogueLine> Fourth1Lines;
    public Unit Objective;
    public List<DialogueLine> Fourth2Lines;
    [Header("Fifth - enemy units and attacks")]
    public List<DialogueLine> Fifth1Lines;
    public Unit EnemyUnit;
    public List<DialogueLine> Fifth2Lines;

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
        //Build();
    }

    #region First - deploying a unit
    IEnumerator First_Deployment()
    {
        Tutorial_SupposedRoundTurn = 0;
        CanClickOnCells = false; Tutorial_CanPass = false;
        bool iswaiting_1 = true; void stopwaiting_1(Unit u) { iswaiting_1 = false; }
        yield return ResourceManager.Instance.InstantiatePlayerUnits();
        ResourceManager.Instance.ResetArmy();

        Action_DeployNewPlayerUnit.E_DeployedNewPlayerUnit.AddListener(stopwaiting_1);

        yield return DialogueManager.Instance.SpeakLines(FirstLines);

        CanClickOnCells = true;
        ActionParameters pars = new ActionParameters(GameManager.ActionType.DeployNew, null, null, null, null, 0);
        yield return GameManager.Instance.Action(pars);

        curDelay = DelayBeforeAutomaticProceeding;
        while (iswaiting_1 && curDelay > 0) { curDelay -= Time.deltaTime; ; yield return new WaitForSeconds(Time.deltaTime); }
        Action_DeployNewPlayerUnit.E_DeployedNewPlayerUnit.RemoveListener(stopwaiting_1);
        Tutorial_SupposedRoundTurn = 1;
        Tutorial_CanPass = true;
    }
    #endregion

    #region Second - passing a turn
    IEnumerator Second_Passing()
    {
        bool iswaiting_2 = true; void stopwaiting_2() { iswaiting_2 = false; }
        PassTurnButton.Instance.E_PassedTurn.AddListener(stopwaiting_2);

        DialogueManager.Instance.StartCoroutine(DialogueManager.Instance.SpeakLines(SecondLines));
        CameraManager.Instance.SetCam(CameraManager.Instance.Left);

        curDelay = DelayBeforeAutomaticProceeding;
        while (iswaiting_2 && curDelay > 0) { curDelay -= Time.deltaTime; ; yield return new WaitForSeconds(Time.deltaTime); }

        PassTurnButton.Instance.E_PassedTurn.RemoveListener(stopwaiting_2);
    }
    #endregion

    #region Third - moving
    IEnumerator Third_Movement()
    {
        bool iswaiting_3 = true; void stopwaiting_3(Unit u) { iswaiting_3 = false; }
        Action_Move.E_AfterMove.AddListener(stopwaiting_3);
        Tutorial_CanPass = false;

        DialogueManager.Instance.StartCoroutine(DialogueManager.Instance.SpeakLines(ThirdLines));

        curDelay = DelayBeforeAutomaticProceeding;
        while (iswaiting_3 && curDelay > 0) { curDelay -= Time.deltaTime; ; yield return new WaitForSeconds(Time.deltaTime); }
        Tutorial_SupposedRoundTurn = 2;
        Action_Move.E_AfterMove.RemoveListener(stopwaiting_3);
        Tutorial_CanPass = true;

        bool iswaiting_32 = true;
        void stopwaiting_32() { iswaiting_32 = false; }
        PassTurnButton.Instance.E_PassedTurn.AddListener(stopwaiting_32);

        curDelay = DelayBeforeAutomaticProceeding;
        while (iswaiting_32 && curDelay > 0) { curDelay -= Time.deltaTime; ; yield return new WaitForSeconds(Time.deltaTime); }

        PassTurnButton.Instance.E_PassedTurn.RemoveListener(stopwaiting_32);
    }
    #endregion

    #region Fourth - objective scoring
    IEnumerator Fourth_Objectives()
    {
        CanClickOnCells = false; Tutorial_CanPass = false;
        yield return DialogueManager.Instance.SpeakLines(Fourth1Lines);

        CameraManager.Instance.SetCam(CameraManager.Instance.TopDown);

        #region Find an appropriate location for an objective
        Unit u = BoardManager.Instance.Get_AllUnitsWithKeywords(new List<Keyword> { Keyword.Player })[0];
        List<Vector2Int> pos = BoardManager.Instance.Get_UnitPositions(u);
        List<Vector2Int> newObjPos = new List<Vector2Int>();
        for (int dir = 0; dir < 4; dir++)
        {
            Vector2Int offset = new Vector2Int(0, 0);
            switch (dir)
            {
                case 0: offset = new Vector2Int(0, -2); break;
                case 1: offset = new Vector2Int(2, 0); break;
                case 2: offset = new Vector2Int(0, 2); break;
                default: offset = new Vector2Int(-2, 0); break;
            }

            newObjPos = new List<Vector2Int> { offset + pos[0] };

            //Debug.Log(newObjPos[0]);
            //You would have to purposly try to break the tutorial for these to matter, but there WILL be one idiot who triggers it
            if (BoardManager.Instance.IsInBounds(newObjPos[0]) && BoardManager.Instance.Board[newObjPos[0].x].Cells[newObjPos[0].y].CurUnit == null)
            {/* Debug.Log("I AM NOT A MORON");*/ break; }
        }
        #endregion

        ActionParameters parsO = new ActionParameters(GameManager.ActionType.Create, new List<Unit> { Objective }, null, newObjPos, null, 0);
        yield return Action_Create.Create(parsO);

        DialogueManager.Instance.StartCoroutine(DialogueManager.Instance.SpeakLines(Fourth2Lines));
        Tutorial_SupposedRoundTurn = 3;
        Tutorial_CanPass = true;

        CanClickOnCells = true;

        #region Waiting until player scores a point
        bool iswaiting_ToScore = true; void stopwaiting_score(Side s) { iswaiting_ToScore = false; }
        Tutorial_SupposedRoundTurn = 2.5f;
        ScoreClock.Instance.e_PointScored.AddListener(stopwaiting_score);

        curDelay = DelayBeforeAutomaticProceeding;
        while (iswaiting_ToScore && curDelay > 0) { curDelay -= Time.deltaTime; yield return new WaitForSeconds(Time.deltaTime); }
        #endregion
    }
    #endregion

    #region Fifth - enemy units and attacks
    IEnumerator Fifth_Enemies()
    {
        Debug.Log("Started fifth");
        CanClickOnCells = false; Tutorial_CanPass = false;
        yield return DialogueManager.Instance.SpeakLines(Fifth1Lines);

        yield return new WaitForSeconds(AestheticDelay);


        #region Find an appropriate location for an enemy unit
        List<Vector2Int> posO = BoardManager.Instance.Get_UnitPositions(Objective);
        List<Vector2Int> newEnemyPos = new List<Vector2Int>();
        for (int dir = 0; dir < 4; dir++)
        {
            Vector2Int offset = new Vector2Int(0, -1);
            switch (dir)
            {
                case 0: offset = new Vector2Int(0, -1); break;
                case 1: offset = new Vector2Int(1, 0); break;
                case 2: offset = new Vector2Int(0, 1); break;
                default: offset = new Vector2Int(-1, 0); break;
            }

            newEnemyPos = new List<Vector2Int> { offset + posO[0] };

            if (BoardManager.Instance.IsInBounds(newEnemyPos[0]) && BoardManager.Instance.Board[newEnemyPos[0].x].Cells[newEnemyPos[0].y].CurUnit == null)
            { Debug.Log("I AM NOT A MORON"); break; }
        }
        #endregion

        Debug.Log("Placing enemy at " + newEnemyPos[0]);
        ActionParameters parsE = new ActionParameters(GameManager.ActionType.Create, new List<Unit> { EnemyUnit }, null, newEnemyPos, null, 1);
        yield return Action_Create.Create(parsE);

        yield return new WaitForSeconds(AestheticDelay);
        yield return DialogueManager.Instance.SpeakLines(Fifth2Lines);

        yield return new WaitForSeconds(AestheticDelay);
        yield return EnemyManager.Instance.MovingAllEnemyUnits();
    }
    #endregion

    IEnumerator StartTutorial()
    {

        yield return new WaitForSeconds(Time.deltaTime);
        yield return First_Deployment();
        
        yield return new WaitForSeconds(Time.deltaTime);
        yield return Second_Passing();

        yield return new WaitForSeconds(Time.deltaTime);
        yield return Third_Movement();

        yield return new WaitForSeconds(Time.deltaTime);
        yield return Fourth_Objectives();

        yield return new WaitForSeconds(Time.deltaTime);
        yield return Fifth_Enemies();

    }

    void Start()
    {
        StartCoroutine( StartTutorial());
    }

}
