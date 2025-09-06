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

    public bool ShouldDebug;


    //Events are called when the turn BEGINS
    public UnityEvent<Side> E_Turn_Functional = new UnityEvent<Side>();
    public UnityEvent<Side> E_Turn_Visuals = new UnityEvent<Side>();

    //Event for the conclusion of the battle 
    public UnityEvent<Side> E_WinOrLoss; //Since i have little object what use it, events will only make code more confusing, so right now this is unused

    public UnityEvent<int> E_RoundStart; //Event for the start of the round

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

    bool CanUnitsAttack()
    {
        if (CurRound == 0) return false;

        return true;
    }
    bool CanUnitsScore()
    {
        if (CurRound == 0) return false;

        return true;
    }

    bool IsChangingTurn = false;
    public IEnumerator SetRoundNTurn(int round, Side turn, bool shouldScore, bool shouldAttack)
    {
        if (ShouldDebug) Debug.Log("SetRoundNTurn");
        if (IsChangingTurn) yield break;
        IsChangingTurn = true;

        if (round == CurRound && CurTurn == turn) { IsChangingTurn = false; yield break; } //If trying to change the round/turn to the same one we are now

        #region Making a side attack and then score with their units
        List<Keyword> sideKeyword = new List<Keyword>();
        if (CurTurn == Side.Player) { sideKeyword.Add(Keyword.Player); } else { { sideKeyword.Add(Keyword.Enemy); } }

        // Debug.Log("AttackFromKeyworded");
        if (CanUnitsAttack() && shouldAttack)
        {
            if (ShouldDebug) Debug.Log("AttackFromKeyworded " + sideKeyword[0]);

            ActionParameters attack = new ActionParameters(
                                GameManager.ActionType.AttackFromKeyworded, null, sideKeyword, null, null, 0);
            yield return Action_AttackFromKeyworded.AttackFromKeyworded(attack);
        }

        yield return new WaitForSeconds(Time.fixedDeltaTime / 10);

        //Debug.Log("GlobalScore");
        if (CanUnitsScore() && shouldScore)
        {
            if (ShouldDebug) Debug.Log("GlobalScore " + sideKeyword[0]);

            ActionParameters score = new ActionParameters(
            GameManager.ActionType.Score, null, sideKeyword, null, null, 0);
            yield return Action_GlobalScore.GlobalScore(score);
        }

        #endregion

        #region handling turn/round change and events for them

        CurTurn = turn;
        CurRound = round;

        E_Turn_Functional.Invoke(BattleStatsManager.Instance.CurTurn);
        E_Turn_Visuals.Invoke(BattleStatsManager.Instance.CurTurn);
        if (ShouldDebug) Debug.Log("E_Turn");

        E_RoundStart.Invoke(round);
        if (ShouldDebug) Debug.Log("E_RoundStart");
        Action_Move.ResetAllUnitsMovement();

        #endregion

        IsChangingTurn = false;
    }

    bool IsChangingToNextTurn = false;
    public IEnumerator NextTurn()
    {
        if (ShouldDebug) Debug.Log("NextTurn");
        yield return new WaitForSeconds(Time.fixedDeltaTime / 100);
        if (IsChangingTurn) yield break;
        IsChangingToNextTurn = true;

        while (IsEnding) { yield return new WaitForSeconds(Time.fixedDeltaTime); }

        Side nT = CurTurn; int nR = CurRound;
        if (nT == Side.Player) { nT = Side.Enemy; } else { nT = Side.Player; nR++; }

        yield return SetRoundNTurn(nR, nT, true, true);

        IsChangingToNextTurn = false;

        yield return new WaitForSeconds(Time.fixedDeltaTime / 100);
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
