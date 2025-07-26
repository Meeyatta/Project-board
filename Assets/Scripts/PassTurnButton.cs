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
    const string Jumpstr = "jump";
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
        Action_NextTurn.E_Turn_Functional.AddListener(EnablePass);
        Action_NextTurn.E_Turn_Visuals.AddListener(Shrug_Visuals);
    }
    void OnDisable()
    {
        Action_NextTurn.E_Turn_Functional.RemoveListener(EnablePass);
        Action_NextTurn.E_Turn_Visuals.RemoveListener(Shrug_Visuals);
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

    public void RemindToPass()
    {
        anim.SetTrigger(Jumpstr);
    }

    bool PassConditions()
    {

        if (Action_DeployPlayerStarters.DeployedPlayerStarters || Action_DeployNewPlayerUnit.IsDeploying ||
            Action_DrawPlayerResources.IsDeployingNewResources || Action_DrawNewItem.IsDrawingNewItem)
            { RuleMessageManager.Instance.PlayMessage(RuleMessageInd.PassBeforeDraw); return false; }

        if (BattleStatsManager.Instance.CurTurn != Side.Player) 
            { RuleMessageManager.Instance.PlayMessage(RuleMessageInd.PassingOnWrongTurn); return false; }

        if (!CanPass || Time.time <= nextClickTime) 
            { return false; }


        return true;
    }

    public void Pass()
    {
        if (TutorialManager.Instance != null) //<- This is purely for tutorial
        {
            if (TutorialCond() && Time.time > nextClickTime)
            {
                nextClickTime = Time.time + ClickReload;
                Shrug_Visuals();
                E_PassedTurn.Invoke();
            }
            
            return;
        }

        //Debug.Log("Button was pressed");
        if (PassConditions()) 
        {
            nextClickTime = Time.time + ClickReload ;
            GameManager.Instance.CancelEvent.Invoke();
            Shrug_Visuals();
            if (BattleStatsManager.Instance.CurRound != 0) CanPass = false;

            //Debug.Log("Button passed the turn");
            ActionParameters parameters = new ActionParameters(GameManager.ActionType.NextTurn, null, null, null, null, 0);
            StartCoroutine( GameManager.Instance.Action(parameters));
            E_PassedTurn.Invoke();
        }
        else if ((TutorialManager.Instance != null && !TutorialManager.Instance.Tutorial_CanPass) ||
            (BattleStatsManager.Instance.CurTurn != Side.Player || !CanPass))
        {
            nextClickTime = Time.time + ClickReload;
        }
    }
    void FixedUpdate()
    {
        if (BattleStatsManager.Instance.CurRound == 0)
        {
            RoundText.text = "Deployment";
            TurnText.text = "Player";
        }
        else
        {
            RoundText.text = BattleStatsManager.Instance.CurRound.ToString();
            TurnText.text = BattleStatsManager.Instance.CurTurn.ToString();
        }


    }
}
