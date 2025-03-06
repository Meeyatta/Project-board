using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

//Can move unit or show possible movement of said unit
public static class Action_Move 
{
    //Returns all possible positions what a unit can move using their current moveset
    //This is used for a single cell unit, but a bunch or redundancies are left from this also being for multicell units
    public static List<List<Vector2Int>> Get_PossibleMovement(Unit unit)
    {
        // Debug.Log("Possible movement positions:");
        List<List<Vector2Int>> res = new List<List<Vector2Int>>();

        #region Checking if a line crosses through another unit
        foreach (Moveset.Line line in unit.CurMoveset.Lines)
        {
            bool isObscured = false;
            for (int i = 0; i < line.Positions.Count; i++)
            {
                string lineS = "";
                List<Vector2Int> iPositions = new List<Vector2Int>();
                foreach (Vector2Int unitP in BoardManager.Instance.Get_UnitPositions(unit))
                {
                    if (!BoardManager.Instance.IsInBounds(line.Positions[i] + unitP) || isObscured) { break; }
                    int x = line.Positions[i].x; int y = line.Positions[i].y;

                    Debug.Log("Going through line " + i + " " + (line.Positions[i] + unitP).x + " " + (line.Positions[i] + unitP).y);

                    if (BoardManager.Instance.Board[unitP.x + x].Cells[unitP.y + y].CurUnit != null &&
                        !BoardManager.Instance.Board[unitP.x + x].Cells[unitP.y + y].CurUnit == unit)
                    {
                        if (unit.CurMoveset.IsEvading) { isObscured = true; }
                        break;
                    }

                    //Debug.Log("     " + (unitP.x + x) + " " + (unitP.y + y) + " cur unit - " + BoardManager.Instance.Board[unitP.x + x].Cells[unitP.y + y].CurUnit);
                    lineS += line.Positions[i] + unitP + " ";
                    iPositions.Add(line.Positions[i] + unitP);
                }

                //For some reason sometimes the code can decide to select positions what the unit shouldn't physically be able to fit in,
                //  which leads to problems, so this check fixes that.
                //  This issue will probably come up later but fuck it I guess. Future me I hope this smug comment was worth your patience 
                if (iPositions.Count == BoardManager.Instance.Get_UnitPositions(unit).Count && !isObscured) { res.Add(iPositions); }

            }
        }



        #endregion
        return res;
    }

    //Returns all possible positions what a multicell unit can move using their current moveset
    //(this needs a separate function because fuck my life)
    public static List<List<List<Vector2Int>>> Get_PossibleMovement_Multi(Unit unit)
    {
        #region Get ALL possible positions in every direction
        List<List<List<Vector2Int>>> results1 = new List<List<List<Vector2Int>>>();
        foreach (var line in unit.CurMoveset.Lines)
        {
            List<List<Vector2Int>> offsetPositions = new List<List<Vector2Int>>();
            #region For all steps in a line, form possible unit positions
            foreach (var step in line.Positions)
            {
                #region Form where the unit would position itself after moving this step through the line
                List<Vector2Int> curStepPos = new List<Vector2Int>();
                foreach (var ps in BoardManager.Instance.Get_UnitPositions(unit))
                {
                    curStepPos.Add(step + ps);
                }
                offsetPositions.Add(curStepPos);
                #endregion
            }
            #endregion
            results1.Add(offsetPositions);
        }
        #endregion

        #region Go through each direction and trim the ones obscured by other things
        List<List<List<Vector2Int>>> results2 = new List<List<List<Vector2Int>>>();
        foreach (var line in results1) 
        {
            #region Going forward in a line
            List<List<Vector2Int>> curLine = new List<List<Vector2Int>>();
            for (int i = 0; i < line.Count; i++)
            {
                bool obscured = false;
                #region check if all positions unit will be placed on are not obsucred
                foreach (var pos in line[i]) 
                {
                    if (BoardManager.Instance.Board[pos.x].Cells[pos.y].CurUnit != null
                        && BoardManager.Instance.Board[pos.x].Cells[pos.y].CurUnit != unit) 
                    { 
                        obscured = true;
                    }                 
                }
                #endregion

                if (obscured && !unit.CurMoveset.IsEvading) { break; }
                if (obscured && unit.CurMoveset.IsEvading) { continue; }
                curLine.Add(line[i]);
            }
            #endregion
            results2.Add(curLine);
        }
        #endregion

        return results2;
    }

    public static IEnumerator Move(Unit ActionTargetUnit, List<Vector2Int> CellsCoordinates)
    {
        if (ActionTargetUnit == null || CellsCoordinates.Count == 0) Debug.LogError("INVALID ACTION PARAMETERS - MOVE(ActionTargetUnit, CellCoordinates)");


        List<Vector2Int> newPoss = new List<Vector2Int>();

        #region If it's a single cell sized unit
        if (ActionTargetUnit.Size.Positions.Count > 1) 
        {
            
            foreach (var v in Get_PossibleMovement_Multi(ActionTargetUnit))
            {
                foreach (var vv in v)
                {
                    if (vv.Intersect<Vector2Int>(CellsCoordinates).Any()) { newPoss = vv; break; }
                }
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
