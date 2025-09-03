using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Action_CreateBattlefield
{
    public static bool ShouldDebug;

    public static IEnumerator CreateBattlefield(int width, int height, 
        List<Vector2Int> objectiveCoords,
        Vector2Int player_deployment_start, Vector2Int player_deployment_end,
        Vector2Int enemy_deployment_start, Vector2Int enemy_deployment_end,
        Vector3 cellsPosition,
        GameObject boardObject, Vector3 boardPosition
        )
    {
        yield return new WaitForSeconds(Time.deltaTime);
        BoardManager.Instance.Build2(width, height,
                                    objectiveCoords,
                                    player_deployment_start, player_deployment_end,
                                    enemy_deployment_start, enemy_deployment_end,
                                    cellsPosition, boardObject, boardPosition);

        #region Placing objectives
        if (objectiveCoords == null || objectiveCoords.Count <= 0) { Debug.LogError("Cannot place objectives: Passed CellsCoordinates must have values"); }

        if (ShouldDebug) Debug.Log("Placing " + objectiveCoords.Count + " objectives");

        for (int i = 0; i < objectiveCoords.Count; i++)
        {
            GameObject objective = BattleManager.Instance.Objective_Obj;
            ActionParameters parametersCreate = new ActionParameters(GameManager.ActionType.Create, null, null, new List<Vector2Int> { objectiveCoords[i] }, objective, -1);

            yield return Action_Create.Create(parametersCreate);
        }
        #endregion

        yield return null;
    }

}
