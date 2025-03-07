using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

//Can move unit or show possible movement of said unit
public static class Action_Move 
{

    public static List<List<Vector2Int>> Get_PossibleMovement_new(Unit u)
    {
        List<List<Vector2Int>> res = new List<List<Vector2Int>>();

        List<Vector2Int> uPos = BoardManager.Instance.Get_UnitPositions(u);
        foreach (var v in uPos) { Debug.Log(v); }


        return null;
    }

    //Returns all possible positions what a unit can move using their current moveset
    //This is used for a single cell unit, but a bunch or redundancies are left from this also being for multicell units
    public static List<List<Vector2Int>> Get_PossibleMovement(Unit unit)
    {
        Get_PossibleMovement_new(unit);

        // Debug.Log("Possible movement positions:");
        List<List<Vector2Int>> res = new List<List<Vector2Int>>();

        #region Checking if a line crosses through another unit
        foreach (Moveset.Line line in unit.CurMoveset.Lines)
        {
            bool isObscured = false;
            //for (int i = 0; i < line.Cells.Count; i++)
            //{
            //    string lineS = "";
            //    List<Vector2Int> iPositions = new List<Vector2Int>();
            //    foreach (Vector2Int pos in line.Cells[i])
            //    {
            //        if (!BoardManager.Instance.IsInBounds(pos + unitP) || isObscured) { break; }
            //        int x = line.Cells[i].x; int y = line.Cells[i].y;

            //        Debug.Log("Going through line " + i + " " + (line.Cells[i] + unitP).x + " " + (line.Cells[i] + unitP).y);

            //        if (BoardManager.Instance.Board[unitP.x + x].Cells[unitP.y + y].CurUnit != null &&
            //            !BoardManager.Instance.Board[unitP.x + x].Cells[unitP.y + y].CurUnit == unit)
            //        {
            //            if (unit.CurMoveset.IsEvading) { isObscured = true; }
            //            break;
            //        }

            //        //Debug.Log("     " + (unitP.x + x) + " " + (unitP.y + y) + " cur unit - " + BoardManager.Instance.Board[unitP.x + x].Cells[unitP.y + y].CurUnit);
            //        lineS += line.Cells[i] + unitP + " ";
            //        iPositions.Add(line.Cells[i] + unitP);
            //    }

            //    //For some reason sometimes the code can decide to select positions what the unit shouldn't physically be able to fit in,
            //    //  which leads to problems, so this check fixes that.
            //    //  This issue will probably come up later but fuck it I guess. Future me I hope this smug comment was worth your patience 
            //    if (iPositions.Count == BoardManager.Instance.Get_UnitPositions(unit).Count && !isObscured) { res.Add(iPositions); }

            //}
        }



        #endregion
        return res;
    }

    public static IEnumerator Move(Unit ActionTargetUnit, List<Vector2Int> CellsCoordinates)
    {
        if (ActionTargetUnit == null || CellsCoordinates.Count == 0) Debug.LogError("INVALID ACTION PARAMETERS - MOVE(ActionTargetUnit, CellCoordinates)");


        List<Vector2Int> newPoss = new List<Vector2Int>();

        #region If it's a single cell sized unit
        if (ActionTargetUnit.Size.Positions.Count > 1) 
        {

            foreach (List<Vector2Int> l in Get_PossibleMovement(ActionTargetUnit))
            {
                if (l.Intersect<Vector2Int>(CellsCoordinates).Any()) { newPoss = l; break; }
            }
        }
        #endregion
        #region else - it's a multicell unit
        else
        {
            foreach (List<Vector2Int> l in Get_PossibleMovement(ActionTargetUnit))
            {
                if (l.Intersect<Vector2Int>(CellsCoordinates).Any()) { newPoss = l; break; }
            }
        }
        #endregion


        if (newPoss.Count > 0)
        {
            yield return GameManager.Instance.StartCoroutine(BoardManager.Instance.MoveUnit(ActionTargetUnit, newPoss));
        }

        yield return new WaitForSeconds(0.001f);
    }

}
