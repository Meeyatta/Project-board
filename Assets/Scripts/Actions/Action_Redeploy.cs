using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//Moves the target unit into position if it is being redeployed during deployment (round 0)
public static class Action_Redeploy 
{
    public static List<List<Vector2Int>> Get_PossibleDeployments(Unit u)
    {
        List<List<Vector2Int>> res = new List<List<Vector2Int>>();

        List<Vector2Int> zone = new List<Vector2Int>();
        if (u.CurKeywords.Contains(Keyword.Player)) { zone = BoardManager.Instance.PlayerDeploymentZone; } else { zone = BoardManager.Instance.EnemyDeploymentZone;}

        #region Going through all deployment zone positions and fitting unit insidie of them
        for (int x = zone[0].x; x <= zone[1].x; x++) 
        {
            for (int y = zone[0].y; y <= zone[1].y; y++)
            {
                List<Vector2Int> positions = u.Size.Positions;
                for (int i = 0; i < positions.Count; i++) { positions[i] += new Vector2Int(x, y); }

                bool isInBounds = true;
                #region Check if all positions are within the deployment zone
                foreach (var v in positions)
                {
                    if (v.x < zone[0].x || v.x > zone[1].x || v.y < zone[0].y || v.y > zone[1].y) { isInBounds = false; break; }
                }
                #endregion

                if (isInBounds) { res.Add(positions); }
            }
        }
        #endregion

        return res;
    }

    public static IEnumerator Redeploy(ActionParameters parameters)
    {
        yield return new WaitForSeconds(Time.deltaTime * 0.5f);

        Unit target = parameters.ActionTargetUnits[0];
        List<Vector2Int> coordinates = parameters.CellsCoordinates;

        List<List<Vector2Int>> posses = Get_PossibleDeployments(target);
        List<Vector2Int> endPos = null;
        foreach (var poss in posses) 
        {
            if (poss.Intersect<Vector2Int>(coordinates).Any()) { endPos = poss; break; }
        }

        if (endPos == null) { yield break; } //If position is not in the deployment zone - do nothing

        #region Moving the unit
        List<Vector2Int> oldPos = BoardManager.Instance.Get_UnitPositions(target);
        foreach (Vector2Int v in oldPos)
        {
            BoardManager.Instance.Board[v.x].Cells[v.y].CurUnit = null;
        }
        yield return BoardManager.Instance.StartCoroutine(BoardManager.Instance.PlaceUnit(target, endPos));
        #endregion

        yield return new WaitForSeconds(Time.deltaTime);
        GameManager.Instance.RemoveAction(parameters);
    }
}
