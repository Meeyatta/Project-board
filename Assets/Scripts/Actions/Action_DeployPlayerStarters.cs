using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Action_DeployPlayerStarters
{
    public static IEnumerator DeployPlayerStarters(ActionParameters parameters)
    {
        #region If we have no units to deploy
        if (ResourceManager.Instance.Army_NotPlaced.Count == 0) 
        {
            yield return new WaitForSeconds(Time.deltaTime);
            GameManager.Instance.RemoveAction(parameters);
            yield break;
        }
        #endregion

        int number = parameters.IntNumber;

        for (int i = 0; i < number; i++)
        {
            EffectManager.Instance.HideAllPlayerMovement(ScoreManager.Instance.CurTurn); // <- Safeguards just in case
            yield return Action_DeployNewPlayerUnit.DeployNewUnit(parameters);
            yield return new WaitForSeconds(Time.deltaTime);
        }

        yield return new WaitForSeconds(Time.deltaTime);
        GameManager.Instance.RemoveAction(parameters);
    }
}
