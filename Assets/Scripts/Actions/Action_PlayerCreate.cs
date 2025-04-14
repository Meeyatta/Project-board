using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public static class Action_PlayerCreate
{
    static string UnitPlacementHolderStr = "UnitPlacementHolder";
    static GameObject UnitPlacementHolderObj;

    public static bool IsWaitingForData;

    public static IEnumerator PlayerCreate(ActionParameters parameters)
    {

        GameObject Object = parameters.Object;

        UnitPlacementHolderObj = GameObject.Find(UnitPlacementHolderStr);
        Vector3 holdPos = Vector3.zero; if (UnitPlacementHolderObj != null) { holdPos = UnitPlacementHolderObj.transform.position; }

        #region Check if an object is already created or needs to be instantiated
        Unit unit = null;
        if (parameters.ActionTargetUnits != null && !parameters.ActionTargetUnits[0].IsPrefab)
        {
            unit = parameters.ActionTargetUnits[0];
        }
        else
        {
            unit = GameManager.Instantiate(Object, holdPos, Quaternion.identity).GetComponent<Unit>();
        }
        #endregion
        List<Unit> unitList = new List<Unit>(); unitList.Add(unit);

        //Add a listener what executes after players selects a position and returns it
        List<Vector2Int> positions = new List<Vector2Int>();

        //ScoreManager.Instance.eTurnEvent_Functional.AddListener(Cancel);
        void StartAwaiting_ListOfPositions(List<Vector2Int> v2)
        {
            IsWaitingForData = false;
            positions = v2;
            Action_SelectPosition.ESendPositionBack.RemoveListener(StartAwaiting_ListOfPositions);
        }
        UnityEvent<List<Vector2Int>> newEv = new UnityEngine.Events.UnityEvent<List<Vector2Int>>() { };
        Action_SelectPosition.ESendPositionBack = newEv;
        Action_SelectPosition.ESendPositionBack.AddListener(StartAwaiting_ListOfPositions);
        IsWaitingForData = true;

        #region Start the action to select a position
        GameManager.Instance.ShowPlacementEvent.Invoke(unitList);
        GameManager.Instance.I_PositionSelect = Action_SelectPosition.Selecting(unit, false);
        yield return GameManager.Instance.StartCoroutine(GameManager.Instance.I_PositionSelect);
        GameManager.Instance.I_PositionSelect = null;
        #endregion

        #region While we are selecting a new position for a unit, hold that unit in a position near player's bag
        //TODO: change the object's position
        #endregion

        //Waiting until we have the data

        while (IsWaitingForData) { yield return new WaitForSeconds(Time.deltaTime * 0.5f); }

        #region Check if can place a unit there
        bool ViablePos = true;
         foreach (var v in positions)
         {
            if (!BoardManager.Instance.IsInBounds(v)) { ViablePos = false; break; }

            if (BoardManager.Instance.Board[v.x].Cells[v.y].CurUnit != null) { ViablePos = false; break; }
         }
        #endregion

        #region Place a unit on said selected positions if they are viable
        GameManager.Instance.HidePlacementEvent.Invoke(unitList);
         if (ViablePos)
         {
             yield return GameManager.Instance.StartCoroutine(BoardManager.Instance.PlaceUnit(unit, positions));
         }
         else
         {
             Debug.Log("Non viable position");
         }
        #endregion



        //ScoreManager.Instance.eTurnEvent_Functional.RemoveListener(Cancel);
        //GameManager.Instance.CancelEvent.RemoveListener(Cancel);
        yield return new WaitForSeconds(0.1f * Time.deltaTime);
        GameManager.Instance.RemoveAction(parameters);
    }

}
