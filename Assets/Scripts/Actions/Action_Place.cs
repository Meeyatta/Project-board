using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public static class Action_Place
{


    public static IEnumerator Place(Unit unit,List<Vector2Int> CellsCoordinates)
    {
        if (!BoardManager.Instance.BoardToWorldPosition(CellsCoordinates).HasValue) 
        {
            Debug.LogError("INVALID PLACING POSITION FOR " + unit.name); yield return null; 
        }

        unit.transform.position = BoardManager.Instance.BoardToWorldPosition(CellsCoordinates).Value;
        unit.transform.rotation = Quaternion.identity;

        bool canPlace = true; 
        foreach (Vector2Int v in CellsCoordinates) 
        { 
            if (BoardManager.Instance.Board[v.x].Cells[v.y].CurUnit != null) { canPlace = false; break; } 
        }

        if (canPlace) { foreach (Vector2Int v in CellsCoordinates) { BoardManager.Instance.Board[v.x].Cells[v.y].CurUnit = unit; } }
        else { Debug.LogError("INVALID PLACING POSITION FOR " + unit.name); }

        yield return new WaitForSeconds(0.001f);
    }

}
