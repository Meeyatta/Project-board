using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*This script is separated into two parts:
    Pre_NextTurn - This handles all units attacking and scoring objectives
    NextTurn - This handles actual turn and round change
*/
public static class Action_NextTurn
{
    public static float Delay = 15;
    static bool isAwaitingNextTurn; //Check if we have already tried getting to next turn and are waiting for it

    static bool ShouldWaitBeforeNextTurn() //If we have to wait for other actions to be done before ending the turn
    {
        /*
        Debug.Log("CurRound: " + ScoreManager.Instance.CurRound + "| CurTurn: " + ScoreManager.Instance.CurTurn +
            "| Tutorial_SupposedRoundTurn: " + TutorialManager.Instance.Tutorial_SupposedRoundTurn);
        */


        if (TutorialManager.Instance != null && ScoreManager.Instance.CurRound >= TutorialManager.Instance.Tutorial_SupposedRoundTurn) 
            { return true; }
        if (TutorialManager.Instance != null && ScoreManager.Instance.CurTurn == Side.Enemy
            && ScoreManager.Instance.CurRound + 0.5f >= TutorialManager.Instance.Tutorial_SupposedRoundTurn)
            { return true; }

        return false;
    }

    public static IEnumerator Pre_nextTurn(ActionParameters parameters)
    {
        //Debug.Log("Started pre NextTurn actions");
        Side CurTurn = ScoreManager.Instance.CurTurn;

        if (ScoreManager.Instance.CurRound == 0 || isAwaitingNextTurn)
        {
            Debug.Log("Canceling next turn action: " + ScoreManager.Instance.CurRound + " " + isAwaitingNextTurn);

            GameManager.Instance.RemoveAction(parameters);
            yield break;
        }

        while (ShouldWaitBeforeNextTurn()) { isAwaitingNextTurn = true; yield return new WaitForSeconds(Time.deltaTime); }
        isAwaitingNextTurn =false;

        switch (CurTurn)
        {
            case Side.Player:
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
            case Side.Enemy:
                #region Enemy turn ended

                ScoreManager.Instance.eTurnEvent_Visuals.Invoke(ScoreManager.Instance.CurTurn);

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
        yield return new WaitForSeconds(Time.deltaTime * Delay);

        #region If the current round is deployment
        if (ScoreManager.Instance.CurRound == 0)
        {
            ScoreManager.Instance.EndDeployment();
        }
        #endregion
        #region For rounds past deployment
        else
        {
            switch (ScoreManager.Instance.CurTurn)
            {
                case Side.Player:
                    #region Player turn ended
                    yield return ScoreManager.Instance.StartCoroutine(ScoreManager.Instance.EndPlayerTurn());
                    break;
                #endregion
                case Side.Enemy:
                    #region Enemy turn ended             
                    yield return ScoreManager.Instance.StartCoroutine(ScoreManager.Instance.EndEnemyTurn());
                    break;
                    #endregion
            }
        }
        
        #endregion

        GameManager.Instance.RemoveAction(parameters);
    }

}
