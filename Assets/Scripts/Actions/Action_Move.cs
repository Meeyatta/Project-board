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

    public static UnityEvent<Unit> E_AfterMove;

    public static bool UnitCanMove(Unit u)
    {
        if (u.Moved) return false;
        //if (ScoreManager.Instance.CurTurn == ScoreManager.Side.Enemy && u.CurKeywords) return false;

        return true;
    }

    public static List<List<List<Vector2Int>>> Get_PossibleMovement(Unit u)
    {
        List<List<List<Vector2Int>>> res1 = new List<List<List<Vector2Int>>>();
        List<Vector2Int> uPos = BoardManager.Instance.Get_UnitPositions(u);

        #region Get ALL positions according to the moveset
        foreach (var v in u.CurMoveset.Lines)
        {
            List<List<Vector2Int>> line = new List<List<Vector2Int>>();
            foreach (var linePos in v.Positions)
            {
                List<Vector2Int> poss = new List<Vector2Int>();
                if (uPos != null && uPos.Count > 0)
                {
                    foreach (var UnitPos in uPos)
                    {
                        poss.Add((UnitPos + linePos));
                    }
                    line.Add(poss);
                }              
            }
            res1.Add(line);
        }
        #endregion

        List<List<List<Vector2Int>>> res2 = new List<List<List<Vector2Int>>>();
        #region Remove positions outside the board
        foreach (var line in res1)
        {
            List<List<Vector2Int>> line2 = new List<List<Vector2Int>>();
            bool isObscured = false;
            foreach (var poss in line)
            {
                foreach (var pos in poss) 
                {
                    if (!BoardManager.Instance.IsInBounds(pos)) { isObscured = true; break; }
                }
                if (!isObscured)
                {
                    line2.Add(poss);
                }
            }
            res2.Add(line2);
        }
        #endregion

        List<List<List<Vector2Int>>> res3 = new List<List<List<Vector2Int>>>();
        #region Remove positions occupied by other units
        foreach (var line in res2)
        {
            List<List<Vector2Int>> line3 = new List<List<Vector2Int>>();
            foreach (var poss in line)
            {
                bool isOccupied = false;
                foreach (var pos in poss)
                {
                    if (BoardManager.Instance.Board[pos.x].Cells[pos.y].CurUnit != null &&
                        BoardManager.Instance.Board[pos.x].Cells[pos.y].CurUnit != u) 
                    {                         
                        isOccupied = true; break; 
                    }
                }
                if (!isOccupied)
                {
                    line3.Add(poss);
                }
            }
            res3.Add(line3);
        }
        #endregion

        #region Debug statements

        //Debug.Log("Moveset: " + u.CurMoveset.Lines + u.CurMoveset.Lines.Count);

        //Debug.Log("Res1:");
        //foreach (var v in res1)
        //{
        //    string l = "";
        //    foreach (var vv in v)
        //    {
        //        foreach (var vvv in vv)
        //        {
        //            l += " " + (vvv);
        //        }
        //        l += "|";
        //    }
        //    Debug.Log(l);
        //}
        //Debug.Log("----");

        //Debug.Log("Res2:");
        //foreach (var v in res2)
        //{
        //    string l = "";
        //    foreach (var vv in v)
        //    {
        //        foreach (var vvv in vv)
        //        {
        //            l += " " + (vvv);
        //        }
        //        l += "|";
        //    }
        //    Debug.Log(l);
        //}
        //Debug.Log("----");

        //Debug.Log("Res3:");
        //foreach (var v in res3)
        //{
        //    string l = "";
        //    foreach (var vv in v)
        //    {
        //        foreach (var vvv in vv)
        //        {
        //            l += " " + (vvv);
        //        }
        //        l += "|";
        //    }
        //    Debug.Log(l);
        //}
        //Debug.Log("----");
        #endregion

        return res3;
    }
    public static List<Vector2Int> Get_PositionsFromSingleCoordinate(Unit ActionTargetUnit, List<Vector2Int> CellsCoordinates)
    {
        List<List<List<Vector2Int>>> res = Get_PossibleMovement(ActionTargetUnit);
  
        for (int i = 0; i < BoardManager.Instance.Height; i++)
        {
            foreach (var line in res)
            {
                if (line.Count <= i || line[i] == null) {  continue; }

                if (line[i].Intersect<Vector2Int>(CellsCoordinates).Any()) { return line[i]; }
            }
        }
        return null;   
    }
    public static IEnumerator Move(ActionParameters parameters)
    {
        Unit ActionTargetUnit = parameters.ActionTargetUnits[0]; 
        List< Vector2Int > CellsCoordinates = parameters.CellsCoordinates;

        if (ActionTargetUnit == null || CellsCoordinates.Count == 0) Debug.LogError("INVALID ACTION PARAMETERS - MOVE(ActionTargetUnit, CellCoordinates)");

        if (!UnitCanMove(ActionTargetUnit)) { yield break; }

        List<Vector2Int> newPoss = Get_PositionsFromSingleCoordinate(ActionTargetUnit, CellsCoordinates);

        if (newPoss != null && newPoss.Count > 0 && !ActionTargetUnit.Moved)
        {
            ActionTargetUnit.Moved = true;
            if (!BoardManager.Instance.AreInBounds(newPoss)) { Debug.LogError("ERROR: POSITION OUT OF BOUNDS"); yield break; }

            List<Vector2Int> oldPos = BoardManager.Instance.Get_UnitPositions(ActionTargetUnit);
            foreach (Vector2Int v in oldPos)
            {
                BoardManager.Instance.Board[v.x].Cells[v.y].CurUnit = null;
            }

            yield return BoardManager.Instance.StartCoroutine(BoardManager.Instance.PlaceUnit(ActionTargetUnit, newPoss));
        }

        Ability_Slippery.Try(ActionTargetUnit);

        yield return new WaitForSeconds(Time.deltaTime);
        GameManager.Instance.RemoveAction(parameters);
    }

}
