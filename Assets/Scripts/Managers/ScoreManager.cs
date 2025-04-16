using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/*

    bool PlayerTurnActionCondition - Quick way for other actions to check if they are performed during the player's turn

*/
public class ScoreManager : MonoBehaviour
{
    //Event for the conclusion of the battle 
    public UnityEvent<Side> WinEvent; //Since i have little object what use it, events will only make code more confusing, so right now
    //This is unused

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


    public UnityEvent<Side> eTurnEvent_Functional;
    public UnityEvent<Side> eTurnEvent_Visuals;
    //public UnityEvent<List<Unit>> EndPlayerTurnEvent;
    public UnityEvent RoundEvent;
    private void Start()
    {
        CurRound = 0;
        CurTurn = Side.Player;

        RoundEvent.AddListener(GameManager.Instance.ResetMovement);
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
        CurTurn = ScoreManager.Side.Enemy;
        eTurnEvent_Functional.Invoke(ScoreManager.Instance.CurTurn);
        //eTurnEvent_Visuals.Invoke(ScoreManager.Instance.CurTurn);

        yield return new WaitForSeconds(Time.deltaTime);
    }
    public IEnumerator EndEnemyTurn()
    {
        CurTurn = ScoreManager.Side.Player;

        eTurnEvent_Functional.Invoke(ScoreManager.Instance.CurTurn);

        yield return StartCoroutine(NextRound());
    }
    public IEnumerator NextRound()
    {
        CurRound++;

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
    }

    void Update()
    {
        #region Checking if any of the side won
        if (Score_Player >= Score_Enemy + NScoreDiff) 
        { 
            WinEvent.Invoke(Side.Player);
            EndScreen.Instance.Win();

            //Debug.Log("Player has won"); 
        }

        if (Score_Enemy >= Score_Player + NScoreDiff) 
        { 
            WinEvent.Invoke(Side.Enemy);
            EndScreen.Instance.Loss();
            //Debug.Log("Enemy has won"); 
        }
        #endregion

    }
}
