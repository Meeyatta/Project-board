using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*This script is separated into two parts:
    Pre_NextTurn - This handles all units attacking and scoring objectives
    NextTurn - This handles actual turn and round change
*/
public static class Action_NextTurn
{
    public static IEnumerator Pre_nextTurn(ActionParameters parameters)
    {
        //Debug.Log("Started pre NextTurn actions");
        ScoreManager.Side CurTurn = ScoreManager.Instance.CurTurn;

        switch (CurTurn)
        {
            case ScoreManager.Side.Player:
                #region Player turn ended

                ActionParameters attackP = new ActionParameters(
                    GameManager.ActionType.AttackFromKeyworded, null, new List<Keyword> { Keyword.Player }, null, null, 0);
                yield return GameManager.Instance.StartCoroutine(GameManager.Instance.Action(attackP));
                //Debug.Log("Initiated attack for the player");

                yield return new WaitForSeconds(0.01f);

                ActionParameters scoreP = new ActionParameters(
                    GameManager.ActionType.Score, null, new List<Keyword> { Keyword.Player }, null, null, 0);
                yield return GameManager.Instance.StartCoroutine(GameManager.Instance.Action(scoreP));
                //Debug.Log("Initiated score for the player");

                break;
            #endregion
            case ScoreManager.Side.Enemy:
                #region Enemy turn ended
                ActionParameters attackE = new ActionParameters(
                    GameManager.ActionType.AttackFromKeyworded, null, new List<Keyword> { Keyword.Enemy }, null, null, 0);
                yield return GameManager.Instance.StartCoroutine(GameManager.Instance.Action(attackE));

                yield return new WaitForSeconds(0.01f);

                ActionParameters scoreE = new ActionParameters(
                    GameManager.ActionType.Score, null, new List<Keyword> { Keyword.Enemy }, null, null, 0);
                yield return GameManager.Instance.StartCoroutine(GameManager.Instance.Action(scoreE));


                break;
                #endregion
        }

        GameManager.Instance.RemoveAction(parameters);
    }

    public static IEnumerator NextTurn(ActionParameters parameters)
    {
        //Debug.Log("Started NextTurn actions");
        switch (ScoreManager.Instance.CurTurn)
        {
            case ScoreManager.Side.Player:
                #region Player turn ended
                ScoreManager.Instance.CurTurn = ScoreManager.Side.Enemy;

                ScoreManager.Instance.TurnEvent.Invoke(ScoreManager.Instance.CurTurn);
                ScoreManager.Instance.TurnText.text = ScoreManager.Instance.CurTurn.ToString();
                break;
            #endregion
            case ScoreManager.Side.Enemy:
                #region Enemy turn ended             
                ScoreManager.Instance.CurTurn = ScoreManager.Side.Player;

                ScoreManager.Instance.TurnEvent.Invoke(ScoreManager.Instance.CurTurn);
                ScoreManager.Instance.TurnText.text = ScoreManager.Instance.CurTurn.ToString();
                yield return ScoreManager.Instance.StartCoroutine(NextRound());

                break;
                #endregion
        }

        //Debug.Log("Sent what ended the nextTurn");
        GameManager.Instance.RemoveAction(parameters);
    }
    static IEnumerator NextRound()
    {
        //Debug.Log("Started Next Round actions");
        ScoreManager.Instance.CurRound++;

        ScoreManager.Instance.RoundText.text = ScoreManager.Instance.CurRound.ToString();
        ScoreManager.Instance.TurnText.text = ScoreManager.Instance.CurTurn.ToString();

        ScoreManager.Instance.RoundEvent.Invoke();
        yield return new WaitForSeconds(0.01f);
    }
}
