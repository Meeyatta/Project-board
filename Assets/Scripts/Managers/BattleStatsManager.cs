using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/*

    bool PlayerTurnActionCondition - Quick way for other actions to check if they are performed during the player's turn

*/
public enum Side { Player, Enemy };
public class BattleStatsManager : MonoBehaviour
{
    //Event for the conclusion of the battle 
    public UnityEvent<Side> E_WinOrLoss; //Since i have little object what use it, events will only make code more confusing, so right now
    //This is unused

    public bool IsEnding;
    public int CurRound;
    public Side CurTurn;

    #region Singleton
    public static BattleStatsManager Instance;
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

    //public UnityEvent<List<Unit>> EndPlayerTurnEvent;
    private void Start()
    {
        GameManager.Instance.E_Restart.AddListener(Restart);
    }

    private void OnDisable()
    {
        GameManager.Instance.E_Restart.RemoveListener(Restart);
    }

    void Restart()
    {
        CurRound = 0;
        CurTurn = Side.Player;
        Score_Player = 0;
        Score_Enemy = 0;
    }

    public void AddPointPlayer()
    {
        if (!IsEnding) Score_Player++;
    }
    public void AddPointEnemy()
    {
        if (!IsEnding) Score_Enemy++;
    }

    //Quick way for other actions to check if they are performed during the player's turn
    public bool PlayerTurnActionCondition()
    {
        if (CurTurn != Side.Player) return false;

        return true;
    }

    void Update()
    {
        #region Checking if any of the side won
        if (Score_Player >= Score_Enemy + NScoreDiff) 
        { 
            E_WinOrLoss.Invoke(Side.Player);
            if (EndScreen.Instance != null) EndScreen.Instance.Win();

            //Debug.Log("Player has won"); 
        }

        if (Score_Enemy >= Score_Player + NScoreDiff) 
        { 
            E_WinOrLoss.Invoke(Side.Enemy);
            if (EndScreen.Instance != null) EndScreen.Instance.Loss();
            //Debug.Log("Enemy has won"); 
        }
        #endregion
    }
}
