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

    [Header("First")]
    public List<DialogueLine> FirstLines;
    [Header("Second")]
    public List<DialogueLine> SecondLines;  
    [Header("Third")]
    public List<DialogueLine> ThirdLines;
    [Header("Fourth")]
    public List<DialogueLine> Fourth1Lines;
    public GameObject Objective;
    public List<DialogueLine> Fourth2Lines;

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

    IEnumerator StartTutorial()
    {
        #region First
        CanClickOnCells = false; Tutorial_CanPass = false;
        bool iswaiting_1 = true; void stopwaiting_1(Unit u) { iswaiting_1 = false; }
        yield return ResourceManager.Instance.InstantiatePlayerUnits();
        ResourceManager.Instance.ResetArmy();

        Action_DeployNewPlayerUnit.E_DeployedNewPlayerUnit.AddListener(stopwaiting_1);

        yield return DialogueManager.Instance.SpeakLines(FirstLines);

        CanClickOnCells = true;

        ActionParameters pars = new ActionParameters(GameManager.ActionType.DeployNew, null, null, null, null, 0);
        yield return GameManager.Instance.Action(pars);

        while (iswaiting_1) { yield return new WaitForSeconds(Time.deltaTime); }
        Action_DeployNewPlayerUnit.E_DeployedNewPlayerUnit.RemoveListener(stopwaiting_1);
         Tutorial_CanPass = true;
        #endregion

        yield return new WaitForSeconds(Time.deltaTime);

        #region Second
        bool iswaiting_2 = true; void stopwaiting_2() { iswaiting_2 = false; }
        PassTurnButton.Instance.E_PassedTurn.AddListener(stopwaiting_2);

        DialogueManager.Instance.StartCoroutine(DialogueManager.Instance.SpeakLines(SecondLines));
        CameraManager.Instance.SetCam(CameraManager.Instance.Left);

        while (iswaiting_2) { yield return new WaitForSeconds(Time.deltaTime); }

        PassTurnButton.Instance.E_PassedTurn.RemoveListener(stopwaiting_2);
        #endregion

        yield return new WaitForSeconds(Time.deltaTime);

        #region Third
        bool iswaiting_3 = true; void stopwaiting_3(Unit u) { iswaiting_3 = false; } 
        Action_Move.E_AfterMove.AddListener(stopwaiting_3);
        Tutorial_CanPass = false;

        DialogueManager.Instance.StartCoroutine(DialogueManager.Instance.SpeakLines(ThirdLines));

        while (iswaiting_3) { yield return new WaitForSeconds(Time.deltaTime); }
        Action_Move.E_AfterMove.RemoveListener(stopwaiting_3);
        Tutorial_CanPass = true;

        bool iswaiting_32 = true;
        void stopwaiting_32() { iswaiting_32 = false; }
        PassTurnButton.Instance.E_PassedTurn.AddListener(stopwaiting_32);

        while (iswaiting_32) { yield return new WaitForSeconds(Time.deltaTime); }

        PassTurnButton.Instance.E_PassedTurn.RemoveListener(stopwaiting_32);

        #endregion

        #region Fourth
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

            Debug.Log(newObjPos[0]);
                //You would have to purposly try to break the tutorial for these to matter, but there WILL be one idiot who triggers it
            if (BoardManager.Instance.IsInBounds(newObjPos[0]) && BoardManager.Instance.Board[newObjPos[0].x].Cells[newObjPos[0].y].CurUnit == null) 
            { Debug.Log("I AM NOT A MORON"); break; }
        }
        #endregion

        ActionParameters parsO = new ActionParameters(GameManager.ActionType.Create, null, null, newObjPos, Objective, 0);
        yield return GameManager.Instance.Action(pars);

        DialogueManager.Instance.StartCoroutine(DialogueManager.Instance.SpeakLines(Fourth2Lines));
        Tutorial_CanPass = true;
        CanClickOnCells = false;
        #endregion

    }

    void Start()
    {
        StartCoroutine( StartTutorial());
    }

}
