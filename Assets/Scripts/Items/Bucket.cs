using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//Base class for all items what apply a cell cover over an area
public static class Bucket 
{
    public static bool ShouldDebug = true;
    #region Pass a 2-count list with a width and height, returns a lsit of relative positions on the board
    static List<Vector2Int> SizeToZone(List<int> zone)
    {
        List<Vector2Int> res = new List<Vector2Int>();
        int[] xy = new int[2];
        if (zone.Count != 2) { Debug.LogError("ERROR: ZoneToCoordinates must receive 2 elements, got " + zone.Count); return res; }

        for (int x = 0; x < zone[0]; x++)
        {
            for (int y = zone[1]; y>0; y--)
            {
                res.Add(new Vector2Int(x, y));
            }
        }

        return res;
    }
    #endregion

    public static IEnumerator CoverArea(EffectManager.Tag showcaseTag, CoverType type, List<int> size)
    {
        //Await Until player clicks on an area
        //While waiting, draw the possible covers under the cursos

        #region Start the action to select a position
        List<Vector2Int> coverCoords = SizeToZone(size);
        //foreach (var v in coverCoords) { Debug.Log("Size coords: " + v); }

        GameManager.Instance.I_PositionSelect = null;
        #endregion

        //Waiting until we have the data
        bool IsWaitingForData = true;
        Vector2Int clickedPosition = Vector2Int.zero;
        void Get_ClickedCellCoordinates(List<Vector2Int> v2)
        {
            clickedPosition = v2[0];
            IsWaitingForData = false;
            Action_SelectUnitPosition.ESendPositionBack.RemoveListener(Get_ClickedCellCoordinates);
        }


        Action_SelectUnitPosition.ESendPositionBack.RemoveListener(Get_ClickedCellCoordinates);
        Action_SelectUnitPosition.ESendPositionBack.AddListener(Get_ClickedCellCoordinates);

        GameManager.Instance.I_PositionSelect = Action_SelectUnitPosition.Selecting_relaxed(coverCoords, false);
        GameManager.Instance.StartCoroutine(GameManager.Instance.I_PositionSelect);
        EffectManager.Instance.StartShowingPossibleZone(coverCoords, showcaseTag);

        if (ShouldDebug) Debug.Log(Action_SelectUnitPosition.ESendPositionBack);
        while (IsWaitingForData) { yield return new WaitForSeconds(Time.fixedDeltaTime * 0.5f); }
        if (ShouldDebug) Debug.Log("Stopped awaiting for click");
        EffectManager.Instance.StopShowingPossibleZone();


        //After player clicks, change the covers of needed cells
    }

}
