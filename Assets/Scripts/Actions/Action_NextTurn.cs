using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/*This script is separated into two parts:
    Pre_NextTurn - This handles all units attacking and scoring objectives
    NextTurn - This handles actual turn and round change
*/
public static class Action_NextTurn
{
    static bool IsChangingTurn;
    static bool IsChangingToNextTurn;

    static bool ShouldDebug = true;

    //Events are called when the turn BEGINS
    public static UnityEvent<Side> E_Turn_Functional = new UnityEvent<Side>();
    public static UnityEvent<Side> E_Turn_Visuals = new UnityEvent<Side>();

    public static UnityEvent<int> E_Round = new UnityEvent<int>();

    static bool CanTheyAttack()
    {
        if (BattleStatsManager.Instance.CurRound == 0) return false;

        return true;
    }
    static bool CanTheyScore()
    {
        if (BattleStatsManager.Instance.CurRound == 0) return false;

        return true;
    }

    public static IEnumerator SetRoundNTurn(int round, Side turn, bool shouldScore, bool shouldAttack)
    {
        if (ShouldDebug) Debug.Log("SetRoundNTurn");
        if (IsChangingTurn) yield break;
        IsChangingTurn = true;

        int curRound = BattleStatsManager.Instance.CurRound;
        Side CurTurn = BattleStatsManager.Instance.CurTurn;

        //Debug.Log("ScoreManager.Instance.CurRound " + ScoreManager.Instance.CurRound);

        if (round == curRound && CurTurn == turn) { IsChangingTurn = false; yield break; } //If trying to change the round/turn to the same one we are now

        #region Making a side attack and then score with their units
        List<Keyword> sideKeyword = new List<Keyword>();
        if (CurTurn == Side.Player) { sideKeyword.Add(Keyword.Player); } else { { sideKeyword.Add(Keyword.Enemy); } }

       // Debug.Log("AttackFromKeyworded");
        if (CanTheyAttack() && shouldAttack)
        {
            if (ShouldDebug) Debug.Log("AttackFromKeyworded " + sideKeyword[0]);

            ActionParameters attack = new ActionParameters(
                                GameManager.ActionType.AttackFromKeyworded, null, sideKeyword, null, null, 0);
            yield return Action_AttackFromKeyworded.AttackFromKeyworded(attack);
        }
        
        yield return new WaitForSeconds(Time.fixedDeltaTime / 10);

        //Debug.Log("GlobalScore");
        if (CanTheyScore() && shouldScore)
        {
            if (ShouldDebug) Debug.Log("GlobalScore " + sideKeyword[0]);

            ActionParameters score = new ActionParameters(
            GameManager.ActionType.Score, null, sideKeyword, null, null, 0);
            yield return Ability_Score.GlobalScore(score);
        }

        #endregion

        #region handling turn/round change and events for them
        
        BattleStatsManager.Instance.CurTurn = turn;
        BattleStatsManager.Instance.CurRound = round;

        E_Turn_Functional.Invoke(BattleStatsManager.Instance.CurTurn);
        E_Turn_Visuals.Invoke(BattleStatsManager.Instance.CurTurn);
        if (ShouldDebug) Debug.Log("E_Turn");

        if (curRound != round) 
        { 
            E_Round.Invoke(round);
            if (ShouldDebug) Debug.Log("E_Round");
            Action_Move.ResetAllUnitsMovement();
        }

        #endregion

        IsChangingTurn = false;
    }
    static int ntc = 0;
    public static IEnumerator NextTurn(ActionParameters parameters)
    {
        Debug.Log("NextTurn");
        yield return new WaitForSeconds(Time.fixedDeltaTime / 100);
        if (IsChangingToNextTurn) yield break;
        IsChangingToNextTurn = true;

        while (BattleStatsManager.Instance.IsEnding) { yield return new WaitForSeconds(Time.fixedDeltaTime); }

        Side nT = BattleStatsManager.Instance.CurTurn; int nR = BattleStatsManager.Instance.CurRound;
        if (nT == Side.Player) { nT = Side.Enemy; } else { nT = Side.Player; nR++; }

        yield return SetRoundNTurn(nR, nT, true, true);

        IsChangingToNextTurn = false;
        GameManager.Instance.RemoveAction(parameters);
    }
}
