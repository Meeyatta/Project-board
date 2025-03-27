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
        #region If the current round is deployment
        if (ScoreManager.Instance.CurRound == 0)
        {
            ScoreManager.Instance.EndDeployment();
            GameManager.Instance.RemoveAction(parameters);
            yield break;
        }
        #endregion

        #region For rounds past deployment
        switch (ScoreManager.Instance.CurTurn)
        {
            case ScoreManager.Side.Player:
                #region Player turn ended
                yield return ScoreManager.Instance.StartCoroutine(ScoreManager.Instance.EndPlayerTurn());
                break;
            #endregion
            case ScoreManager.Side.Enemy:
                #region Enemy turn ended             
                yield return ScoreManager.Instance.StartCoroutine(ScoreManager.Instance.EndEnemyTurn());
                break;
                #endregion
        }
        #endregion

        GameManager.Instance.RemoveAction(parameters);
    }

}
