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

    //Events are called when the turn BEGINS
    public static UnityEvent<Side> eTurnEvent_Functional = new UnityEvent<Side>();
    public static UnityEvent<Side> eTurnEvent_Visuals = new UnityEvent<Side>();

    public static UnityEvent<int> RoundEvent = new UnityEvent<int>();

    static bool CanTheyAttack()
    {
        if (ScoreManager.Instance.CurRound == 0) return false;

        return true;
    }
    static bool CanTheyScore()
    {
        if (ScoreManager.Instance.CurRound == 0) return false;

        return true;
    }

    public static IEnumerator SetRoundNTurn(int round, Side turn, bool shouldScore, bool shouldAttack)
    {
        Debug.Log("SetRoundNTurn");

        IsChangingTurn = true;
        int curRound = ScoreManager.Instance.CurRound;
        Side CurTurn = ScoreManager.Instance.CurTurn;

        Debug.Log("ScoreManager.Instance.CurRound " + ScoreManager.Instance.CurRound);

        if (round == curRound && CurTurn == turn) { IsChangingTurn = false; yield break; } //If trying to change the round/turn to the same one we are now

        #region Making a side attack and then score with their units
        List<Keyword> sideKeyword = new List<Keyword>();
        if (CurTurn == Side.Player) { sideKeyword.Add(Keyword.Player); } else { { sideKeyword.Add(Keyword.Enemy); } }

        Debug.Log("AttackFromKeyworded");
        if (CanTheyAttack() && shouldAttack)
        {
            Debug.Log("Can AttackFromKeyworded " + ScoreManager.Instance.CurRound);

            ActionParameters attack = new ActionParameters(
                                GameManager.ActionType.AttackFromKeyworded, null, sideKeyword, null, null, 0);
            yield return Action_AttackFromKeyworded.AttackFromKeyworded(attack);
        }
        
        yield return new WaitForSeconds(Time.fixedDeltaTime);

        Debug.Log("GlobalScore");
        if (CanTheyScore() && shouldScore)
        {
            Debug.Log("Can GlobalScore");

            ActionParameters score = new ActionParameters(
            GameManager.ActionType.Score, null, sideKeyword, null, null, 0);
            yield return Ability_Score.GlobalScore(score);
        }

        #endregion

        #region handling turn/round change and events for them
        
        ScoreManager.Instance.CurTurn = turn;
        ScoreManager.Instance.CurRound = round;

        eTurnEvent_Functional.Invoke(ScoreManager.Instance.CurTurn);
        eTurnEvent_Visuals.Invoke(ScoreManager.Instance.CurTurn);

        if (curRound != round) 
        { 
            RoundEvent.Invoke(round);
            Action_Move.ResetAllUnitsMovement();
        }

        #endregion

        IsChangingTurn = false;

        Debug.Log("Ended SetRoundNTurn");
        yield return new WaitForSeconds(Time.fixedDeltaTime);
    }

    public static IEnumerator NextTurn(ActionParameters parameters)
    {
        Side nT = ScoreManager.Instance.CurTurn; int nR = ScoreManager.Instance.CurRound;
        if (nT == Side.Player) { nT = Side.Enemy; } else { nT = Side.Player; nR++; }

        yield return SetRoundNTurn(nR, nT, true, true);

        yield return new WaitForSeconds(0.1f * Time.fixedDeltaTime);
        GameManager.Instance.RemoveAction(parameters);
    }
}
