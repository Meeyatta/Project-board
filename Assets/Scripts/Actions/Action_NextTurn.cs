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
    public static float Delay = 15;
    public static bool IsChangingTurn; //Check if we have already tried getting to next turn and are waiting for it

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
        IsChangingTurn = true;
        int curRound = ScoreManager.Instance.CurRound;
        Side CurTurn = ScoreManager.Instance.CurTurn;

        if (round == curRound && CurTurn == turn) { IsChangingTurn = false; yield break; } //If trying to change the round/turn to the same one we are now

        #region Making a side attack and then score with their units
        List<Keyword> sideKeyword = new List<Keyword>();
        if (CurTurn == Side.Player) { sideKeyword.Add(Keyword.Player); } else { { sideKeyword.Add(Keyword.Enemy); } }

        if (CanTheyAttack() && shouldAttack)
        {
            ActionParameters attack = new ActionParameters(
                                GameManager.ActionType.AttackFromKeyworded, null, sideKeyword, null, null, 0);
            yield return GameManager.Instance.StartCoroutine(GameManager.Instance.Action(attack));
        }
        
        yield return new WaitForSeconds(Time.deltaTime);

        if (CanTheyScore() && shouldScore)
        {
            ActionParameters score = new ActionParameters(
            GameManager.ActionType.Score, null, sideKeyword, null, null, 0);
            yield return GameManager.Instance.Action(score);
        }
        
        #endregion

        yield return new WaitForSeconds(Time.deltaTime * Delay);

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

        yield return new WaitForSeconds(Time.deltaTime);
    }

    public static IEnumerator Pre_nextTurn(ActionParameters parameters)
    {

        Side CurTurn = ScoreManager.Instance.CurTurn;

        if (ScoreManager.Instance.CurRound == 0 || IsChangingTurn)
        {
            Debug.Log("Canceling next turn action: " + ScoreManager.Instance.CurRound + " " + IsChangingTurn);

            GameManager.Instance.RemoveAction(parameters);
            yield break;
        }

        #region Making a side attack and then score with their units
        IsChangingTurn = true;
        List<Keyword> sideKeyword = new List<Keyword>();
        if (CurTurn == Side.Player) { sideKeyword.Add(Keyword.Player); } else { { sideKeyword.Add(Keyword.Enemy); } }

        ActionParameters attack = new ActionParameters(
                    GameManager.ActionType.AttackFromKeyworded, null, sideKeyword, null, null, 0);
        yield return GameManager.Instance.StartCoroutine(GameManager.Instance.Action(attack));

        yield return new WaitForSeconds(Time.deltaTime);

        ActionParameters scoreP = new ActionParameters(
            GameManager.ActionType.Score, null, sideKeyword, null, null, 0);
        yield return GameManager.Instance.StartCoroutine(GameManager.Instance.Action(scoreP));
        #endregion


        GameManager.Instance.RemoveAction(parameters);
    }

    public static IEnumerator NextTurn(ActionParameters parameters)
    {
        Side newTurn = ScoreManager.Instance.CurTurn; int newRound = ScoreManager.Instance.CurRound;

        if (ScoreManager.Instance.CurTurn == Side.Enemy) { newTurn = Side.Player; newRound += 1; }
        else { newTurn = Side.Enemy; }
        
        yield return SetRoundNTurn(newRound, newTurn, true, true);

        GameManager.Instance.RemoveAction(parameters);
    }

}
