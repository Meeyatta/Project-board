using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//Base class for all items what apply a cell cover over an area
public static class Bucket 
{
    //Pass a 2-count list with a width and height, returns a lsit of relative positions on the board
    static List<Vector2Int> ZoneToCoordinates(List<int> zone)
    {
        List<Vector2Int> res = new List<Vector2Int>();
        int[] xy = new int[2];
        if (zone.Count != 2) { Debug.LogError("ERROR: ZoneToCoordinates must receive 2 elements, got " + zone.Count); return res; }

        for (int x = 0; x < xy[0]; x++)
        {
            for (int y = xy[1]; y>0; y++)
            {
                res.Add(new Vector2Int(x, y));
            }
        }

        return res;
    }

    public static IEnumerator CoverArea(CoverType type, List<int> zone)
    {
        //Await Until player clicks on an area
        //While waiting, draw the possible covers under the cursos

        #region Start the action to select a position
        List<Vector2Int> coverCoords = ZoneToCoordinates(zone);
        GameManager.Instance.I_PositionSelect = Action_SelectUnitPosition.Selecting_relaxed(coverCoords, false);
        yield return GameManager.Instance.StartCoroutine(GameManager.Instance.I_PositionSelect);

        GameManager.Instance.I_PositionSelect = null;
        #endregion

        //Waiting until we have the data
        bool IsWaitingForData = true;
        Vector2Int clickedPosition = Vector2Int.zero;
        void Get_ClickedCellCoordinates(List<Vector2Int> v2)
        {
            clickedPosition = v2[0];
            Action_SelectUnitPosition.ESendPositionBack.RemoveListener(Get_ClickedCellCoordinates);
            
        }

        UnityEvent<List<Vector2Int>> newEv = new UnityEngine.Events.UnityEvent<List<Vector2Int>>() { };
        Action_SelectUnitPosition.ESendPositionBack = newEv;
        Action_SelectUnitPosition.ESendPositionBack.AddListener(Get_ClickedCellCoordinates);

        while (IsWaitingForData) { yield return new WaitForSeconds(Time.fixedDeltaTime * 0.5f); }

        //After player clicks, change the covers of needed cells
    }

}
