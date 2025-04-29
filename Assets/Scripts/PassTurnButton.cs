using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class PassTurnButton : MonoBehaviour
{
    public TextMeshProUGUI RoundText;
    public TextMeshProUGUI TurnText;

    public float ClickReload; float nextClickTime = 1;
    public bool CanPass = true;

    const string Shrugstr = "shrug";
    Animator anim;

    public UnityEvent E_PassedTurn;

    #region Singleton
    public static PassTurnButton Instance;
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
        anim = GetComponent<Animator>();
    }
    #endregion

    #region Subscribing/Unsubscribing events
    void SubscribeEvents()
    {
        Action_NextTurn.eTurnEvent_Functional.AddListener(EnablePass);
        Action_NextTurn.eTurnEvent_Visuals.AddListener(Shrug_Visuals);
    }
    void OnDisable()
    {
        Action_NextTurn.eTurnEvent_Functional.RemoveListener(EnablePass);
        Action_NextTurn.eTurnEvent_Visuals.RemoveListener(Shrug_Visuals);
    }
    #endregion

    void Start()
    {
        SubscribeEvents();

        RoundText.text = "Deployment";
        TurnText.text = "-";     
    }

    public void PlaySound(SoundName n)
    {
        AudioManager.Instance.Play(n, transform);
    }

    #region Visual effects for when the turnButton is activated, for in-script use and event
    public void Shrug_Visuals()
    {
       anim.SetTrigger(Shrugstr);
    }
    public void Shrug_Visuals(Side s)
    {
        //if (s == Side.Enemy) return;
        //Shrug_Visuals();
    }
    #endregion

    void EnablePass(Side side)
    {
        if (side == Side.Player) 
        {
            CanPass = true;
        }
    }

    bool TutorialCond()
    {
        return (TutorialManager.Instance != null && TutorialManager.Instance.Tutorial_CanPass);
    }

    public void Pass()
    {
        if (TutorialManager.Instance != null) //<- This is purely for tutorial
        {
            if (TutorialCond() && Time.time > nextClickTime)
            {
                nextClickTime = Time.time + ClickReload;
                Debug.Log("Is passing in tutorial");
                Shrug_Visuals();
                E_PassedTurn.Invoke();
            }
            
            return;
        }

        //Debug.Log("Button passed the turn");
        if (ScoreManager.Instance.CurTurn == Side.Player && CanPass && Time.time > nextClickTime) 
        {
            nextClickTime = Time.time + ClickReload ;
            GameManager.Instance.CancelEvent.Invoke();
            Shrug_Visuals();
            if (ScoreManager.Instance.CurRound != 0) CanPass = false;

            ActionParameters parameters = new ActionParameters(GameManager.ActionType.NextTurn, null, null, null, null, 0);
            StartCoroutine(GameManager.Instance.Action(parameters));
            E_PassedTurn.Invoke();
        }
        else if ((TutorialManager.Instance != null && !TutorialManager.Instance.Tutorial_CanPass) ||
            (ScoreManager.Instance.CurTurn != Side.Player || !CanPass))
        {
            nextClickTime = Time.time + ClickReload;
        }
    }
    void FixedUpdate()
    {
        if (ScoreManager.Instance.CurRound == 0)
        {
            RoundText.text = "Deployment";
            TurnText.text = "Player";
        }
        else
        {
            RoundText.text = ScoreManager.Instance.CurRound.ToString();
            TurnText.text = ScoreManager.Instance.CurTurn.ToString();
        }


    }
}
