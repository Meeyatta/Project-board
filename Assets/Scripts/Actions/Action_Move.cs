using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public static class Action_Move 
{


    public static IEnumerator Move(Unit ActionTargetUnit, List<Vector2Int> CellsCoordinates)
    {
        if (ActionTargetUnit == null || CellsCoordinates.Count == 0) Debug.LogError("INVALID ACTION PARAMETERS - MOVE(ActionTargetUnit, CellCoordinates)");


        List<Vector2Int> newPoss = new List<Vector2Int>();
        foreach (List<Vector2Int> l in GameManager.Instance.GetPossibleMovement(ActionTargetUnit))
        {
            if (l.Intersect<Vector2Int>(CellsCoordinates).Any()) { newPoss = l; break; }
        }

        string newPossS = "Moving to new positions";
        foreach (Vector2Int v in newPoss)
        {
            newPossS += v + " ";
        }
        //Debug.Log(newPossS);

        if (newPoss.Count > 0)
        {
            yield return GameManager.Instance.StartCoroutine(BoardManager.Instance.MoveUnit(ActionTargetUnit, newPoss));
        }

        yield return new WaitForSeconds(0.001f);
    }

}
