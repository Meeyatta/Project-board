using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Action_Deployment 
{
    //Gets random remaining positions inside the deployment zone
    static List<Vector2Int> randPos(Unit unit, List<Cell> remainingCells, int minX, int maxX, int minY, int maxY)
    {
        Vector2Int topLeftPos = new Vector2Int(Random.Range(minX, maxX), Random.Range(minY, maxY));

        //For some ungodly reason when I set "res" to "unit.Size.Positions" directly it started modifying the size positions,
        //even when I refered to "res". WTF
        List<Vector2Int> res = new List<Vector2Int>();
        for (int i = 0; i < unit.Size.Positions.Count; i++) { res.Add(unit.Size.Positions[i] + topLeftPos); }

        return res;
    }

    //Randomly places units from inputed roster within the coordinates
    public static IEnumerator Deploy(ActionParameters parameters)
    {
        yield return new WaitForSeconds(5f * Time.deltaTime);

        List<Unit> roster = parameters.ActionTargetUnits;
        int amount = parameters.IntNumber;
        List<Vector2Int> deploymentZone = parameters.CellsCoordinates;


        //Debug.Log(parameters);
        //Debug.Log(roster[0].UnitName + " " + deploymentZone[0]);

        int miX = deploymentZone[0].x; int maX = deploymentZone[1].x;
        int miY = deploymentZone[0].y; int maY = deploymentZone[1].y;

        #region Record the deployment zone
        List<Cell> deploymentZoneCells = new List<Cell>();
        for (int x = miX; x < maX; x++)
        {
            for (int y = miY; y < maY; y++)
            {
                deploymentZoneCells.Add(BoardManager.Instance.Board[x].Cells[y]);
            }
        }
        #endregion

        #region Going through every enemy and placing them in the deployment zone
        List<Cell> remainingCells = deploymentZoneCells;

        int unitsLeft = amount;
        foreach (var unit in roster)
        {
            if (remainingCells.Count < unit.Size.Positions.Count) { break; } //If ran out of space completely

            #region Trying to fit unit in random positions, if tried to do it 999 times and failed - means there is no space left
            int safeGuard = 999; 
            List<Vector2Int> newPos = randPos(unit, remainingCells, miX, maX, miY, maY);
            while ((!BoardManager.Instance.AreInBounds(newPos) || !BoardManager.Instance.AreAnyOccupied(newPos))
                && unitsLeft > 0 && safeGuard > 0)
            {
                newPos = randPos(unit, remainingCells, miX, maX, miY, maY);
                safeGuard--;
                yield return new WaitForSeconds(0.001f);
            }

            if (unitsLeft <= 0) { Debug.Log("Ran out of units"); break; }
            if (safeGuard <= 0) { Debug.Log("Failed to fit unit"); break; }
            #endregion

            unitsLeft--;
            yield return BoardManager.Instance.StartCoroutine(BoardManager.Instance.PlaceUnit(unit, newPos));
            //Debug.Log("Post placing the unit");
        }
        #endregion

        yield return new WaitForSeconds(Time.deltaTime);
        GameManager.Instance.RemoveAction(parameters);
    }
}
