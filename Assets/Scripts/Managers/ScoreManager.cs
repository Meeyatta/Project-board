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

    public UnityEvent<Side> TurnEvent;
    public UnityEvent RoundEvent;
    private void Start()
    {
        CurRound = 0;
        CurTurn = Side.Player;

        RoundText.text = "Deployment";
        TurnText.text = "Player";
        RoundEvent.AddListener(GameManager.Instance.ResetMovement);
    }
    public void EndDeployment()
    {
        CurRound = 1;
        CurTurn = Side.Player;

        RoundText.text = CurRound.ToString();
    }

    void Update()
    {
        if (Score_Player >= Score_Enemy + NScoreDiff) { WinEvent.Invoke(Side.Player); Debug.Log("Player has won"); }

        if (Score_Enemy >= Score_Player + NScoreDiff) { WinEvent.Invoke(Side.Enemy); Debug.Log("Enemy has won"); }

    }
}
