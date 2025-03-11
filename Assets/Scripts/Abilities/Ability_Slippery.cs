using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class Ability_Slippery
{
    public static void Try(Unit u)
    {
        if (!u.CurAbilities.Contains(Ability.Slippery)) { return; }

        ActionParameters parameters = new ActionParameters(GameManager.ActionType.Slip, new List<Unit> {u}, null, null, null);
        GameManager.Instance.StartCoroutine(GameManager.Instance.Action(parameters));
    }
    public static List<Vector2Int> GetRandPos(Unit u)
    {
        List<Vector2Int> res = new List<Vector2Int>();
        int dir = Random.Range(0, 8); //Get the direction in which unit slips

        switch (dir)
        {
            #region Up
            case 0:
                res = BoardManager.Instance.Get_UnitPositions(u);
                for (int i = 0; i < res.Count; i++) { res[i] += new Vector2Int(0, -1); }
                break;
            #endregion
            #region Up-right
            case 1:
                res = BoardManager.Instance.Get_UnitPositions(u);
                for (int i = 0; i < res.Count; i++) { res[i] += new Vector2Int(1, -1); }
                break;
            #endregion
            #region Right
            case 2:
                res = BoardManager.Instance.Get_UnitPositions(u);
                for (int i = 0; i < res.Count; i++) { res[i] += new Vector2Int(1, 0); }
                break;
            #endregion
            #region Right-down
            case 3:
                res = BoardManager.Instance.Get_UnitPositions(u);
                for (int i = 0; i < res.Count; i++) { res[i] += new Vector2Int(1, 1); }
                break;
            #endregion
            #region Down
            case 4:
                res = BoardManager.Instance.Get_UnitPositions(u);
                for (int i = 0; i < res.Count; i++) { res[i] += new Vector2Int(0, 1); }
                break;
            #endregion
            #region Down-left
            case 5:
                res = BoardManager.Instance.Get_UnitPositions(u);
                for (int i = 0; i < res.Count; i++) { res[i] += new Vector2Int(-1, 1); }
                break;
            #endregion
            #region Left
            case 6:
                res = BoardManager.Instance.Get_UnitPositions(u);
                for (int i = 0; i < res.Count; i++) { res[i] += new Vector2Int(-1, 0); }
                break;
            #endregion
            #region Left-up
            default:
                res = BoardManager.Instance.Get_UnitPositions(u);
                for (int i = 0; i < res.Count; i++) { res[i] += new Vector2Int(-1, -1); }
                break;
                #endregion
        }

        return res;
    }
    public static IEnumerator Slip(ActionParameters parameters)
    {
        Unit unit = parameters.ActionTargetUnits[0];
        List<Vector2Int> endPoss = GetRandPos(unit);
        bool IsValid = BoardManager.Instance.AreInBounds(endPoss) && BoardManager.Instance.AreAnyOccupied(endPoss);

        yield return new WaitForSeconds(0.4f);

        if (IsValid) 
        {
            List<Vector2Int> oldPos = BoardManager.Instance.Get_UnitPositions(unit);
            foreach (Vector2Int v in oldPos)
            {
                BoardManager.Instance.Board[v.x].Cells[v.y].CurUnit = null;
            }

            yield return BoardManager.Instance.StartCoroutine(BoardManager.Instance.PlaceUnit(unit, endPoss));
        }

        yield return new WaitForSeconds(0.01f);
        GameManager.Instance.RemoveAction(parameters);
    }

}
