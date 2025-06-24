using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Action_CreateBattlefield 
{
    public static bool ShouldDebug;
    public static IEnumerator CreateBattlefield(ActionParameters parameters)
    {
        yield return new WaitForSeconds(Time.deltaTime);

        #region Placing objectives
        if (parameters.CellsCoordinates == null || parameters.CellsCoordinates.Count <= 0) { Debug.LogError("Cannot place objectives: Passed CellsCoordinates must have values"); }

        List<Vector2Int> Coords = parameters.CellsCoordinates;
        if (ShouldDebug) Debug.Log("Placing " + Coords.Count + " objectives");

        for (int i = 0; i < parameters.CellsCoordinates.Count; i++) 
        {
            GameObject objective = BattlefieldManager.Instance.Objective_Obj;
            ActionParameters parametersCreate = new ActionParameters(GameManager.ActionType.Create, null, null, new List<Vector2Int> { Coords[i] }, objective, -1);

            yield return Action_Create.Create(parametersCreate);
        }
        #endregion

        yield return new WaitForSeconds(Time.deltaTime);
        GameManager.Instance.RemoveAction(parameters);
    }
    
}
