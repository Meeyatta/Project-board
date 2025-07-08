using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//Base class for all items what apply a cell cover over an area
public static class Bucket 
{
    public static bool ShouldDebug = false;
    public static bool ShouldCancel = false;

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

    static void Cancel()
    {
        ShouldCancel = true;
    }

    static void Cancel_cleanup()
    {
        ItemManagement.Instance.CurItem.gameObject.SetActive(true);
        GameManager.Instance.CancelEvent.RemoveListener(Cancel);
    }

    public static IEnumerator CoverArea(EffectManager.Tag showcaseTag, CoverType coverType, List<int> size)
    {
        GameManager.Instance.CancelEvent.RemoveListener(Cancel);
        GameManager.Instance.CancelEvent.AddListener(Cancel);
        ShouldCancel = false;

        #region Start the action to select a position
        List<Vector2Int> coverCoords = SizeToZone(size);
        //foreach (var v in coverCoords) { Debug.Log("Size coords: " + v); }

        GameManager.Instance.I_PositionSelect = null;
        #endregion

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

        //Waiting until we have the data
        while (IsWaitingForData && !ShouldCancel) 
        { 
            yield return new WaitForSeconds(Time.fixedDeltaTime * 0.1f);
            
            if (ShouldCancel) 
            { 
                Cancel_cleanup();  
                yield return null; 
            }
        }

        if (ShouldDebug) Debug.Log("Stopped awaiting for click");
        EffectManager.Instance.StopShowingPossibleZone();

        #region Covering the actual zone
        if (!ShouldCancel)
        {
            Vector2Int center = BoardManager.Instance.Get_CenterOfZone(coverCoords);
            foreach (var v in coverCoords)
            {
                Vector2Int newCoord = BoardManager.Instance.CursorToCellPosition() + (v - center);
                BoardManager.Instance.Board[newCoord.x].Cells[newCoord.y].CoveredBy = coverType;
            }

            #region Cleanup
            GameManager.Instance.CancelEvent.RemoveListener(Cancel);
            ItemManagement.Instance.SpendCurItem(ItemManagement.Instance.CurItem);
            #endregion
        }
        #endregion


        //After player clicks, change the covers of needed cells
    }

}
