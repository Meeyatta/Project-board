using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    //Event for the conclusion of the battle 
    public UnityEvent<Side> WinEvent;

    public int CurRound;
    public Side CurTurn;
    public enum Side { Player, Enemy };

    #region Singleton
    public static ScoreManager Instance;
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
    #endregion

    public int NScoreDiff;
    public int Score_Player;
    public int Score_Enemy;

    [Header("------")]
    public TextMeshProUGUI RoundText;
    public TextMeshProUGUI TurnText;

    private void Start()
    {
        CurRound = 1;
        CurTurn = Side.Player;

        RoundText.text = "1";
        TurnText.text = "Player";
        RoundEvent.AddListener(GameManager.Instance.ResetMovement);
    }

    #region Turn
    //Swaps to the next turn (Player-Enemy)
    public UnityEvent<Side> TurnEvent;
    void NextTurn()
    {
        switch (CurTurn)
        {
            case Side.Player:
                ActionParameters scoreP = new ActionParameters(GameManager.ActionType.Score, null, new List<Unit.Keyword> { Unit.Keyword.Player }, null, null);
                GameManager.Instance.ActionWrapper(scoreP);
                CurTurn = Side.Enemy;

                TurnEvent.Invoke(CurTurn);
                TurnText.text = CurTurn.ToString();
                break;
            case Side.Enemy:
                ActionParameters scoreE = new ActionParameters(GameManager.ActionType.Score, null, new List<Unit.Keyword> { Unit.Keyword.Enemy }, null, null);
                GameManager.Instance.ActionWrapper(scoreE);
                CurTurn = Side.Player;

                TurnEvent.Invoke(CurTurn);
                TurnText.text = CurTurn.ToString();
                NextRound();
                break;
        }
    }
    #endregion

    //Swaps to the next round of combat
    public UnityEvent RoundEvent;
    void NextRound()
    {
        CurRound++;

        RoundText.text = CurRound.ToString();
        TurnText.text = CurTurn.ToString();

        RoundEvent.Invoke();
    }

    void Update()
    {
        if (Score_Player >= Score_Enemy + NScoreDiff) { WinEvent.Invoke(Side.Player); Debug.Log("Player has won"); }

        if (Score_Enemy >= Score_Player + NScoreDiff) { WinEvent.Invoke(Side.Enemy); Debug.Log("Enemy has won"); }

        if (Input.GetKeyDown("s"))
        {
            NextTurn();
        }
    }
}
