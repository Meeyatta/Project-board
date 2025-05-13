using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Action_CreateBattlefield 
{
    public static IEnumerator CreateBattlefield(ActionParameters parameters)
    {
        yield return new WaitForSeconds(Time.deltaTime);

        #region Placing objectives
        for (int i = 0; i < parameters.CellsCoordinates.Count; i++) 
        {
            GameObject objective = BattlefieldManager.Instance.Objective_Obj;
            ActionParameters parametersCreate = new ActionParameters(
            GameManager.ActionType.Create, null, null, parameters.CellsCoordinates, objective, 0);

            yield return Action_Create.Create(parametersCreate);
        }
        #endregion

        yield return new WaitForSeconds(Time.deltaTime);
        GameManager.Instance.RemoveAction(parameters);
    }
    
}
