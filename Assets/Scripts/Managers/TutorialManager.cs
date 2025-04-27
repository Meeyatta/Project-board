using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;
using UnityEngine.SceneManagement;

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
    public List<DialogueLine> FourthScoldLines;
    public List<DialogueLine> Fourth2Lines;
    [Header("Fifth - enemy units and attacks")]
    public List<DialogueLine> Fifth1Lines;
    public Unit EnemyUnit;
    public List<DialogueLine> Fifth2Lines;
    [Header("Sixth - win/loss")]
    public Unit NewUnit;
    public List<DialogueLine> Sixth1Lines;
    public List<DialogueLine> SixthWinLines;
    public List<DialogueLine> SixthLossLines;
    public string NormalGameSceneName;

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

    void StopInteracting() { CanClickOnCells = false; Tutorial_CanPass = false; }
    void ResumeInteracting() { CanClickOnCells = true; Tutorial_CanPass = true; }

    #region First - deploying a unit
    IEnumerator First_Deployment()
    {
        Debug.Log("Started first");

        StopInteracting();

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
        ResumeInteracting();

        Debug.Log("Ended first");
    }
    #endregion

    #region Second - passing a turn
    IEnumerator Second_Passing()
    {
        Debug.Log("Started second");

        bool iswaiting_2 = true; void stopwaiting_2() { iswaiting_2 = false; }
        PassTurnButton.Instance.E_PassedTurn.AddListener(stopwaiting_2);

        StopInteracting();
        yield return DialogueManager.Instance.SpeakLines(SecondLines);
        ResumeInteracting();
        CameraManager.Instance.SetCam(CameraManager.Instance.Left);

        curDelay = DelayBeforeAutomaticProceeding;
        while (iswaiting_2 && curDelay > 0) { curDelay -= Time.deltaTime; ; yield return new WaitForSeconds(Time.deltaTime); }

        PassTurnButton.Instance.E_PassedTurn.RemoveListener(stopwaiting_2);

        Debug.Log("Ended second");
    }
    #endregion

    #region Third - moving
    IEnumerator Third_Movement()
    {
        Debug.Log("Started third");

        StopInteracting();
        DialogueManager.Instance.StartCoroutine(DialogueManager.Instance.SpeakLines(ThirdLines));
        CanClickOnCells = true;


        bool iswaiting_3 = true; void stopwaiting_3(Unit u) 
        { 
            iswaiting_3 = false;
        }
        Action_Move.E_AfterMove.AddListener(stopwaiting_3);

        while (iswaiting_3) { yield return new WaitForSeconds(Time.deltaTime); }

        ResumeInteracting();
        bool iswaiting_32 = true;
        void stopwaiting_32() { iswaiting_32 = false; }
        PassTurnButton.Instance.E_PassedTurn.AddListener(stopwaiting_32);

        while (iswaiting_32) { yield return new WaitForSeconds(Time.deltaTime); }

        Action_Move.E_AfterMove.RemoveListener(stopwaiting_3);
        PassTurnButton.Instance.E_PassedTurn.RemoveListener(stopwaiting_32);

        Debug.Log("Ended third");
    }
    #endregion

    #region Fourth - objective scoring
    IEnumerator Fourth_Objectives()
    {

        Debug.Log("Started fourth");

        StopInteracting();
        yield return DialogueManager.Instance.SpeakLines(Fourth1Lines);
        CanClickOnCells = true;

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

        ActionParameters parsO = new ActionParameters(GameManager.ActionType.Create, new List<Unit> { Objective }, null, newObjPos, null, -1);
        yield return Action_Create.Create(parsO);

        StopInteracting();
        yield return DialogueManager.Instance.SpeakLines(Fourth2Lines);
        ResumeInteracting();

        #region This checks if player decided NOT to move unit towards the objective
        void stopmovingto(Unit u)
        {
            Unit pU = BoardManager.Instance.Get_AllUnitsWithKeywords(new List<Keyword> { Keyword.Player })[0];
            Vector3 oP = BoardManager.Instance.BoardToWorldPosition(BoardManager.Instance.Get_UnitPositions(Objective)).Value;
            Vector3 pP = BoardManager.Instance.BoardToWorldPosition(BoardManager.Instance.Get_UnitPositions(pU)).Value;

            //If the distance between them is too great, scold the player and make them try again
            if (Vector3.Distance(oP, pP) > 2.14f)
            {
                StopInteracting();
                StartCoroutine( DialogueManager.Instance.SpeakLines(FourthScoldLines));
                ResumeInteracting();
                pU.Moved = false;
            }
        }
        Action_Move.E_AfterMove.AddListener(stopmovingto);
        #endregion

        #region Waiting until player scores a point
        bool iswaiting_ToScore = true; void stopwaiting_score() { iswaiting_ToScore = false; }
        Ability_Score.E_ScoredForPlayer.AddListener(stopwaiting_score);

        void ScorePlayer()
        {
            ActionParameters score = new ActionParameters(GameManager.ActionType.Score, null, new List<Keyword> { Keyword.Player }, null, null, 0);
            GameManager.Instance.StartCoroutine(GameManager.Instance.Action(score));
        }

        PassTurnButton.Instance.E_PassedTurn.AddListener(ScorePlayer);
        while (iswaiting_ToScore ) 
        {
            yield return new WaitForSeconds(Time.deltaTime); 
        }
        #endregion

        Action_Move.E_AfterMove.RemoveListener(stopmovingto);
        Ability_Score.E_ScoredForPlayer.RemoveListener(stopwaiting_score);
        PassTurnButton.Instance.E_PassedTurn.RemoveListener(ScorePlayer);
        Debug.Log("Ended fourth");
    }
    #endregion

    #region Fifth - enemy units and attacks
    IEnumerator Fifth_Enemies()
    {
        Debug.Log("Started fifth");

        StopInteracting();
        yield return DialogueManager.Instance.SpeakLines(Fifth1Lines);
        Debug.Log("Stopped speaking fifth1lines, started finding location for unit");

        #region Find an appropriate location for an enemy unit
        Debug.Log("Started looking for an enemy position");
        List<Vector2Int> posP = BoardManager.Instance.Get_UnitPositions(
            BoardManager.Instance.Get_AllUnitsWithKeywords(new List<Keyword> { Keyword.Player })[0]);
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

            newEnemyPos = new List<Vector2Int> { offset + posP[0] };

            if (BoardManager.Instance.IsInBounds(newEnemyPos[0]) &&
                BoardManager.Instance.Board[newEnemyPos[0].x].Cells[newEnemyPos[0].y].CurUnit == null)
            { break; }
        }
        Debug.Log("Stopped looking for an enemy position");
        #endregion

        Debug.Log("Placing enemy at ");
        AudioManager.Instance.Play(SoundName.Step, transform);
        ActionParameters parsE = new ActionParameters(GameManager.ActionType.Create, new List<Unit> { EnemyUnit }, null, newEnemyPos, null, 1);
        yield return Action_Create.Create(parsE);
        Debug.Log("Ended placing enemy at " + newEnemyPos[0]);

        yield return new WaitForSeconds(AestheticDelay);
        yield return DialogueManager.Instance.SpeakLines(Fifth2Lines);

        ActionParameters parsA = new ActionParameters(GameManager.ActionType.AttackFromKeyworded, null, new List<Keyword> {Keyword.Enemy }, null, null, 0);
        yield return Action_AttackFromKeyworded.AttackFromKeyworded(parsA);

        yield return new WaitForSeconds(AestheticDelay);

        Debug.Log("Ended fifth");
    }
    #endregion

    #region Sixth - win/loss
    IEnumerator Sixth_win_loss()
    {
        Debug.Log("Started sixth");
        bool isWaitingForgameEnd = true; Side s = Side.Enemy;
        void GameEnd(Side side) { s = side; isWaitingForgameEnd = false; }
        ScoreManager.Instance.E_WinOrLoss.AddListener(GameEnd);

        void NormalPass() 
        {
            GameManager.Instance.CancelEvent.Invoke();

            StartCoroutine( Action_NextTurn.SetRoundNTurn(ScoreManager.Instance.CurRound+1, Side.Player, true, true));
        }
        PassTurnButton.Instance.E_PassedTurn.AddListener(NormalPass);

        StopInteracting();
        yield return DialogueManager.Instance.SpeakLines(Sixth1Lines);
        ResumeInteracting();

        ResourceManager.Instance.AddUnit(NewUnit);
        NewUnit.SetToPlayer();
        yield return new WaitForSeconds(Time.deltaTime);

        ActionParameters pars = new ActionParameters(GameManager.ActionType.DeployNew, null, null, null, null, 0);
        yield return GameManager.Instance.Action(pars);

        while (isWaitingForgameEnd) { yield return new WaitForSeconds(Time.deltaTime); }

        if (s == Side.Player)
        { //If player won
            yield return DialogueManager.Instance.SpeakLines(SixthWinLines);
        }
        else
        { //If enemy won
            yield return DialogueManager.Instance.SpeakLines(SixthLossLines);
        }

        yield return new WaitForSeconds(AestheticDelay);
        //Passing onto the real battle
        var scene = SceneManager.LoadSceneAsync(NormalGameSceneName);
        scene.allowSceneActivation = false;
        do
        {
            yield return new WaitForSeconds(Time.deltaTime);
        }
        while (scene.progress < 0.9f);
        scene.allowSceneActivation = true;

        ScoreManager.Instance.E_WinOrLoss.RemoveListener(GameEnd);
        PassTurnButton.Instance.E_PassedTurn.RemoveListener(NormalPass);
    }
    #endregion

    IEnumerator StartTutorial()
    {

        yield return new WaitForSeconds(Time.deltaTime);
        yield return First_Deployment();

        yield return new WaitForSeconds(Time.deltaTime);
        yield return Second_Passing();

        yield return new WaitForSeconds(Time.deltaTime);
        yield return Action_NextTurn.SetRoundNTurn(1, Side.Player, false, false);

        yield return new WaitForSeconds(Time.deltaTime);
        yield return Third_Movement();

        yield return new WaitForSeconds(Time.deltaTime);
        yield return Action_NextTurn.SetRoundNTurn(2, Side.Player, false, false);

        yield return new WaitForSeconds(Time.deltaTime);
        yield return Fourth_Objectives();

        yield return new WaitForSeconds(Time.deltaTime);
        yield return Fifth_Enemies();

        yield return new WaitForSeconds(Time.deltaTime);
        yield return Action_NextTurn.SetRoundNTurn(3, Side.Player, false, false);

        yield return new WaitForSeconds(Time.deltaTime);
        yield return Sixth_win_loss();

    }

    void Start()
    {
        StartCoroutine( StartTutorial());
    }

}
