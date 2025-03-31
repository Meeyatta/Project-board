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
        //STOPPED HERES
        return false;
    }

    #region Goes through possible positions unit can be redeployed to and picks the one closest to the unit
    public static List<Vector2Int> Get_MovementPositionsFromSingleCoordinate(Unit ActionTargetUnit, List<Vector2Int> CellsCoordinates)
    {
        List<Vector2Int> res = new List<Vector2Int>();
        List<Vector2Int> zone = BoardManager.Instance.PlayerDeploymentZone;
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

    public static List<List<Vector2Int>> Get_PossibleDeployments(Unit u)
    {
        List<List<Vector2Int>> res = new List<List<Vector2Int>>();

        List<Vector2Int> zone = new List<Vector2Int>();
        if (u.CurKeywords.Contains(Keyword.Player)) { zone.AddRange(BoardManager.Instance.PlayerDeploymentZone); } 
        else { zone.AddRange(BoardManager.Instance.EnemyDeploymentZone);}

        #region Going through all deployment zone positions and fitting unit insidie of them
        for (int x = zone[0].x; x <= zone[1].x; x++) 
        {
            for (int y = zone[0].y; y <= zone[1].y; y++)
            {
                List<Vector2Int> positions = new List<Vector2Int>(); positions.AddRange( u.Size.Positions);
                for (int i = 0; i < positions.Count; i++) { positions[i] += new Vector2Int(x, y); }

                bool isApplicable = true;
                #region Check if all positions are within the deployment zone
                foreach (var v in positions)
                {
                    if (v.x < zone[0].x || v.x > zone[1].x || v.y < zone[0].y || v.y > zone[1].y) { isApplicable = false; break; }
                    if (BoardManager.Instance.Board[x].Cells[y].CurUnit != null) { isApplicable = false; break; }
                }
                #endregion

                if (isApplicable) { res.Add(positions); }
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

        Debug.Log("Redeploying " + target.UnitName + " to " + coordinates[0]);

        List<List<Vector2Int>> posses = Get_PossibleDeployments(target);

        // |    Scary vodoo expression I've copy-pasted
        // v
        if (!posses.Any(p => p.SequenceEqual(coordinates)) || BoardManager.Instance.AreAnyOccupied(coordinates))
        {
            Debug.Log("Positions do not fit in the deployment zone");
            yield break;
        }

        #region Moving the unit
        List<Vector2Int> oldPos = BoardManager.Instance.Get_UnitPositions(target);
        if (oldPos != null && oldPos.Count > 0) 
        {
            foreach (Vector2Int v in oldPos)
            {
                BoardManager.Instance.Board[v.x].Cells[v.y].CurUnit = null;
            }
        }
        
        yield return BoardManager.Instance.StartCoroutine(BoardManager.Instance.PlaceUnit(target, coordinates));
        AudioManager.Instance.Play(SoundName.Step, target.transform);
        #endregion

        yield return new WaitForSeconds(Time.deltaTime);
        GameManager.Instance.RemoveAction(parameters);
    }
}
