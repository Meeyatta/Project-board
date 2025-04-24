using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class Action_Create
{
    public static IEnumerator Create(ActionParameters parameters)
    {
        yield return new WaitForSeconds(Time.deltaTime);
        GameObject Object = parameters.Object; List<Vector2Int> poss = parameters.CellsCoordinates;
        int side = parameters.IntNumber;

        #region Checking if we need to instantiate a new unit or if we can use an old one
        Unit unit = new Unit();
        if (parameters.ActionTargetUnits != null && parameters.ActionTargetUnits.Count > 0) 
        {
            unit = parameters.ActionTargetUnits[0]; 
        }
        else
        {
            unit = GameManager.Instantiate(Object, Vector3.zero, Quaternion.identity).GetComponent<Unit>();
        }
        #endregion

        if (side == 0) { unit.SetToPlayer(); } else { unit.SetToEnemy(); }

        bool ViablePos = true;
        foreach (var v in poss)
        {
            if (!BoardManager.Instance.IsInBounds(v)) { ViablePos = false; break; }

            if (BoardManager.Instance.Board[v.x].Cells[v.y].CurUnit != null) { ViablePos = false; break; }
        }

        //Debug.Log("ViablePos is: " + ViablePos);
        if (ViablePos)
        {
            yield return GameManager.Instance.StartCoroutine(BoardManager.Instance.PlaceUnit(unit, poss));
        }
        else
        {
            Debug.Log("Non viable position");
        }

        yield return new WaitForSeconds(Time.deltaTime);
        GameManager.Instance.RemoveAction(parameters);
    }
}
