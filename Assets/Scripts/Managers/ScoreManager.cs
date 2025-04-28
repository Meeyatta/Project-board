using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/*

    bool PlayerTurnActionCondition - Quick way for other actions to check if they are performed during the player's turn

*/
public enum Side { Player, Enemy };
public class ScoreManager : MonoBehaviour
{
    //Event for the conclusion of the battle 
    public UnityEvent<Side> E_WinOrLoss; //Since i have little object what use it, events will only make code more confusing, so right now
    //This is unused

    public int CurRound;
    public Side CurTurn;

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

    //public UnityEvent<List<Unit>> EndPlayerTurnEvent;
    private void Start()
    {
        CurRound = 0;
        CurTurn = Side.Player;

    }
    public void AddPointPlayer()
    {
        Score_Player++;
    }
    public void AddPointEnemy()
    {
        Score_Enemy++;
    }
    public IEnumerator EndPlayerTurn()
    {
        CurTurn = Side.Enemy;
        Action_NextTurn.eTurnEvent_Functional.Invoke(ScoreManager.Instance.CurTurn);
        //eTurnEvent_Visuals.Invoke(ScoreManager.Instance.CurTurn);

        yield return new WaitForSeconds(Time.fixedDeltaTime);
    }
    public IEnumerator EndEnemyTurn()
    {
        CurTurn = Side.Player;

        Action_NextTurn.eTurnEvent_Functional.Invoke(ScoreManager.Instance.CurTurn);

        yield return StartCoroutine(NextRound());
    }
    public IEnumerator NextRound()
    {
        yield return new WaitForSeconds(Time.fixedDeltaTime);
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
