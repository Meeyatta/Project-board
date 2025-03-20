using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

/*

    bool PlayerTurnActionCondition - Quick way for other actions to check if they are performed during the player's turn

*/
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

    public TextMeshProUGUI PlayerPointsText;
    public TextMeshProUGUI SymbolText; //This is the symbol between player and enemy points to show the relative number
    public TextMeshProUGUI EnemyPointsText;

    public UnityEvent<Side> TurnEvent;
    public UnityEvent<List<Unit>> EndPlayerTurnEvent;
    public UnityEvent RoundEvent;
    private void Start()
    {
        CurRound = 0;
        CurTurn = Side.Player;

        RoundText.text = "Deployment";
        TurnText.text = "Player";
        RoundEvent.AddListener(GameManager.Instance.ResetMovement);
    }
    public IEnumerator EndPlayerTurn()
    {
        CurTurn = ScoreManager.Side.Enemy;
        TurnEvent.Invoke(ScoreManager.Instance.CurTurn);
        TurnText.text = ScoreManager.Instance.CurTurn.ToString();
        EndPlayerTurnEvent.Invoke( BoardManager.Instance.Get_AllUnitsWithKeywords(new List<Keyword> { Keyword.Player }));

        yield return new WaitForSeconds(Time.deltaTime);
    }
    public IEnumerator EndEnemyTurn()
    {
        CurTurn = ScoreManager.Side.Player;

        TurnText.text = CurTurn.ToString();
        yield return StartCoroutine(NextRound());
        TurnEvent.Invoke(CurTurn);
    }
    public IEnumerator NextRound()
    {
        //Debug.Log("Started Next Round actions");
        CurRound++;

        RoundText.text = ScoreManager.Instance.CurRound.ToString();
        TurnText.text = ScoreManager.Instance.CurTurn.ToString();

        RoundEvent.Invoke();
        yield return new WaitForSeconds(Time.deltaTime);
    }

    //Quick way for other actions to check if they are performed during the player's turn
    public bool PlayerTurnActionCondition()
    {
        if (CurTurn != Side.Player) return false;

        return true;
    }
    public void EndDeployment()
    {
        CurRound = 1;
        CurTurn = Side.Player;

        RoundText.text = CurRound.ToString();
    }

    void Update()
    {
        #region Displaying current points
        PlayerPointsText.text = Score_Player.ToString();
        EnemyPointsText.text = Score_Enemy.ToString();
        if (Score_Player == Score_Enemy) { SymbolText.text = "="; }
        else {
            string res = (Score_Player - Score_Enemy).ToString(); if (Score_Player > Score_Enemy) { res = "+" + res; }
            SymbolText.text = res; }
        #endregion

        #region Checking if any of the side won
        if (Score_Player >= Score_Enemy + NScoreDiff) 
        { 
            WinEvent.Invoke(Side.Player); 
            //Debug.Log("Player has won"); 
        }

        if (Score_Enemy >= Score_Player + NScoreDiff) 
        { 
            WinEvent.Invoke(Side.Enemy); 
            //Debug.Log("Enemy has won"); 
        }
        #endregion

    }
}
