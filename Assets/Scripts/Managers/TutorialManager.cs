using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Frist")]
    public List<Vector2Int> IntroPos;
    public GameObject FirstUnit;
    public List<DialogueLine> FirstLines;
    [Header("2nd")]
    public List<DialogueLine> ScndLines;
    [Header("3rd")]
    public List<DialogueLine> ThrdLines;


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
        yield return DialogueManager.Instance.SpeakLines(FirstLines);

        CameraManager.Instance.SetCam(CameraManager.Instance.InFrontOfBoad);

        ActionParameters pars = new ActionParameters(GameManager.ActionType.Create, null, null, IntroPos, FirstUnit, 0);
        yield return GameManager.Instance.Action(pars);
        #endregion

        #region 2nd
        ScoreManager.Instance.CurRound = 1;
        bool moved = false;
        void Moved(Unit u) { moved = true; }
        Action_Move.E_AfterMove.AddListener(Moved);

        yield return DialogueManager.Instance.SpeakLines(ScndLines);
        while (!moved) { yield return new WaitForSeconds(Time.deltaTime); }
        #endregion

        #region 3rd
        bool passed = false;
        void Passed() { passed = true; }
        PassTurnButton.Instance.E_PassedTurn.AddListener(Passed);

        yield return DialogueManager.Instance.SpeakLines(ThrdLines);

        while (!passed) { yield return new WaitForSeconds(Time.deltaTime); }
        #endregion

        yield return new WaitForSeconds(3);


    }

    

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine( StartTutorial());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
