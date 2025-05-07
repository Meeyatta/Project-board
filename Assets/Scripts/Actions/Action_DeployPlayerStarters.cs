using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Action_DeployPlayerStarters
{
    public static bool DeployedPlayerStarters = false;

    static void Restart()
    {
        DeployedPlayerStarters = false;
    }

    public static IEnumerator DeployPlayerStarters(ActionParameters parameters)
    {
        #region If we have no units to deploy
        if (PlayerManager.Instance.Army_NotPlaced.Count == 0 || DeployedPlayerStarters) 
        {
            yield return new WaitForSeconds(Time.fixedDeltaTime);
            GameManager.Instance.RemoveAction(parameters);
            yield break;
        }
        #endregion

        int number = parameters.IntNumber;

        GameManager.Instance.E_Restart.RemoveListener(Restart);
        GameManager.Instance.E_Restart.AddListener(Restart);
        DeployedPlayerStarters = true;

        for (int i = 0; i < number; i++)
        {
            ActionParameters p = new ActionParameters(GameManager.ActionType.DeployNew, null, null, null, null, 0);
            EffectManager.Instance.HideAllPlayerMovement(ScoreManager.Instance.CurTurn); // <- Safeguards just in case
            yield return Action_DeployNewPlayerUnit.DeployNewUnit(p);
            yield return new WaitForSeconds(Time.deltaTime);

        }

        yield return new WaitForSeconds(Time.deltaTime);
        GameManager.Instance.RemoveAction(parameters);
    }
}
