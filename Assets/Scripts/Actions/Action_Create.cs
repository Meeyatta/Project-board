using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class Action_Create
{
    public static IEnumerator Create(ActionParameters parameters)
    {
        yield return new WaitForSeconds(0.001f);
        Unit unit = parameters.ActionTargetUnits[0]; List<Vector2Int> poss = parameters.CellsCoordinates;

        if (unit.IsPrefab) unit = GameManager.Instantiate(unit.gameObject, Vector3.zero, Quaternion.identity).GetComponent<Unit>();
        List<Unit> unitList = new List<Unit>(); unitList.Add(unit);

        bool ViablePos = true;
        foreach (var v in poss)
        {
            if (!BoardManager.Instance.IsInBounds(v)) { ViablePos = false; break; }

            if (BoardManager.Instance.Board[v.x].Cells[v.y].CurUnit != null) { ViablePos = false; break; }
        }
        if (ViablePos)
        {
            yield return GameManager.Instance.StartCoroutine(BoardManager.Instance.PlaceUnit(unit, poss));
        }
        else
        {
            Debug.Log("Non viable position");
        }

        yield return new WaitForSeconds(0.001f);
        GameManager.Instance.RemoveAction(parameters);
    }
}
