using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public static class Action_Place
{
    public static IEnumerator Place(ActionParameters parameters)
    {
        Unit unit = parameters.ActionTargetUnits[0]; List<Vector2Int> CellsCoordinates = parameters.CellsCoordinates;

        if (!BoardManager.Instance.BoardToWorldPosition(CellsCoordinates).HasValue) 
        {
            Debug.LogError("INVALID PLACING POSITION FOR " + unit.name); yield return null; 
        }

        Debug.Log("Placing " + unit.gameObject.name + " on " + BoardManager.Instance.BoardToWorldPosition(CellsCoordinates).Value);
        unit.transform.position = BoardManager.Instance.BoardToWorldPosition(CellsCoordinates).Value;
        unit.transform.rotation = Quaternion.identity;

        bool canPlace = true; 
        foreach (Vector2Int v in CellsCoordinates) 
        { 
            if (BoardManager.Instance.Board[v.x].Cells[v.y].CurUnit != null) { canPlace = false; break; } 
        }

        if (canPlace) { foreach (Vector2Int v in CellsCoordinates) { BoardManager.Instance.Board[v.x].Cells[v.y].CurUnit = unit; } }
        else { Debug.LogError("INVALID PLACING POSITION FOR " + unit.name); }

        yield return new WaitForSeconds(Time.fixedDeltaTime);
        GameManager.Instance.RemoveAction(parameters);

    }

}
