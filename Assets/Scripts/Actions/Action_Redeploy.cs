using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//Moves the target unit into position if it is being redeployed during deployment (round 0)
public static class Action_Redeploy
{
    static bool RedeployCond()
    {
        return false;
    }

    #region Goes through possible positions unit can be redeployed to and picks the one closest to the unit
    public static List<Vector2Int> Get_MovementPositionsFromSingleCoordinate(Unit ActionTargetUnit, List<Vector2Int> CellsCoordinates)
    {
        List<Vector2Int> res = new List<Vector2Int>();
        List<Vector2Int> zone = new List<Vector2Int> { BoardManager.Instance.PlayerDeployment_start, BoardManager.Instance.PlayerDeployment_end };
        for (int x = zone[0].x; x <= zone[1].x; x++)
        {
            for (int y = zone[0].y; y <= zone[1].y; y++)
            {
                res.Add(new Vector2Int(x, y));
            }
        }

        #region Find the closest one to the player
        float minDist = Mathf.Infinity;
        List<Vector2Int> lastP = new List<Vector2Int>();
        foreach (var p in res)
        {
            if (Vector3.Distance(
                BoardManager.Instance.BoardToWorldPosition(new List<Vector2Int> { p }).Value,
                BoardManager.Instance.BoardToWorldPosition(CellsCoordinates).Value)
                < minDist)
            {

            }
        }
        #endregion

        return null;
    }
    #endregion

    //Returns the list of possible positions unit can be redeployed to
    public static List<List<Vector2Int>> Get_PossibleDeployments(Unit u)
    {
        List<List<Vector2Int>> res = new List<List<Vector2Int>>();

        List<Vector2Int> zone = u.CurKeywords.Contains(Keyword.Player)
        ? new List<Vector2Int> { BoardManager.Instance.PlayerDeployment_start, BoardManager.Instance.PlayerDeployment_end }
        : new List<Vector2Int> { BoardManager.Instance.EnemyDeployment_start, BoardManager.Instance.EnemyDeployment_end };

        #region Going through all deployment zone positions and fitting unit inside of them
        for (int x = zone[0].x; x <= zone[1].x; x++)
        {
            for (int y = zone[0].y; y <= zone[1].y; y++)
            {
                List<Vector2Int> positions = new List<Vector2Int>(); positions.AddRange(u.Size.Positions);
                for (int i = 0; i < positions.Count; i++) { positions[i] += new Vector2Int(x, y); }

                bool isApplicable = true;
                #region Check if all positions are within the deployment zone

                foreach (var v in positions)
                {
                    if (v.x < zone[0].x || v.x > zone[1].x || v.y < zone[0].y || v.y > zone[1].y)
                    {
                        isApplicable = false; break;
                    }
                    if (BoardManager.Instance.Board[v.x].Cells[v.y].CurUnit != null && BoardManager.Instance.Board[v.x].Cells[v.y].CurUnit != u) { isApplicable = false; break; }
                }
                #endregion

                if (isApplicable) { res.Add(positions); }
                else { }
            }
        }
        #endregion

        return res;
    }

    static List<Vector2Int> CanBeRedeployed(Unit unit, Vector2Int clickedCoords)
    {
        List<Vector2Int> desiredPos = new List<Vector2Int>(); 
        foreach (var s in unit.Size.Positions)
        {
            desiredPos.Add(clickedCoords + s);
        }

        List<Vector2Int> bestFit_pos = null;
        int bestFit_index = 0;

        foreach (var posSet in Get_PossibleDeployments(unit))
        {
            int ind = 0;
            foreach (var v in desiredPos)
            {
                if (posSet.Contains(v)) { ind++; }
            }

            if (ind == unit.Size.Positions.Count) return posSet;

            if (ind > bestFit_index)
            {
                bestFit_pos = posSet;
                bestFit_index = ind;
            }
        }

        return bestFit_pos;
    }

    public static IEnumerator Redeploy(Unit unit, Vector2Int clickedCoord)
    {
        yield return new WaitForSeconds(Time.deltaTime * 0.5f);

        //Debug.Log("Redeploying " + target.UnitName + " to " + coordinates[0]);

        List<Vector2Int> position = CanBeRedeployed(unit, clickedCoord);

        if (position == null) yield break;

        #region Redeploying the unit
        List<Vector2Int> oldPos = BoardManager.Instance.Get_UnitPositions(unit);
        if (oldPos != null && oldPos.Count > 0)
        {
            foreach (Vector2Int v in oldPos)
            {
                BoardManager.Instance.Board[v.x].Cells[v.y].CurUnit = null;
            }
        }

        yield return BoardManager.Instance.StartCoroutine(BoardManager.Instance.PlaceUnit(unit, position));
        AudioManager.Instance.Play(SoundName.Step, unit.transform);
        #endregion
    }

    public static IEnumerator Redeploy(ActionParameters parameters)
    {
        yield return Redeploy(parameters.ActionTargetUnits[0], parameters.CellsCoordinates[0]);

        yield return new WaitForSeconds(Time.deltaTime);
        GameManager.Instance.RemoveAction(parameters);
    }
}