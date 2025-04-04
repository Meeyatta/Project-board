using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

public class PassTurnButton : MonoBehaviour
{
    public TextMeshProUGUI RoundText;
    public TextMeshProUGUI TurnText;

    public GameObject Button;
    public bool CanPass = true;

    string Shrugstr = "Shrug";
    Animator anim;

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
        ScoreManager.Instance.eTurnEvent_Functional.AddListener(EnablePass);
        ScoreManager.Instance.eTurnEvent_Visuals.AddListener(Shrug_Visuals);
    }
    void OnDisable()
    {
        ScoreManager.Instance.eTurnEvent_Functional.RemoveListener(EnablePass);
        ScoreManager.Instance.eTurnEvent_Visuals.RemoveListener(Shrug_Visuals);
    }
    #endregion

    void Start()
    {
        SubscribeEvents();

        RoundText.text = "Deployment";
        TurnText.text = "-";     
    }

    #region Visual effects for when the turnButton is activated, for in-script use and event
    public void Shrug_Visuals()
    {
        anim.SetTrigger(Shrugstr);
        AudioManager.Instance.Play(SoundName.ClockPling, transform);
    }
    public void Shrug_Visuals(ScoreManager.Side s)
    {
        //if (s == ScoreManager.Side.Enemy) return;
        Shrug_Visuals();
    }
    #endregion

    void EnablePass(ScoreManager.Side side)
    {
        if (side == ScoreManager.Side.Player) 
        {
            CanPass = true;
        }
    }
    public void Pass()
    {
        if (ScoreManager.Instance.CurTurn == ScoreManager.Side.Player && CanPass)
        {
            Shrug_Visuals();
            if (ScoreManager.Instance.CurRound != 0) CanPass = false;

            ActionParameters parameters = new ActionParameters(GameManager.ActionType.NextTurn, null, null, null, null, 0);
            StartCoroutine(GameManager.Instance.Action(parameters));
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


        Button.transform.LookAt(Camera.main.transform.position);
    }
}
