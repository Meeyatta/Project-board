using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public static class Action_PlayerCreate
{
    public static bool IsWaitingForData;
    static bool ShouldCancel = false;
    static void Cancel()
    {
        ShouldCancel = true;
    }
    public static IEnumerator PlayerCreate(ActionParameters parameters)
    {
        GameObject Object = parameters.Object;
        GameManager.Instance.CancelEvent.AddListener(Cancel);

        //Create a unit as an object, it's not on the board yet, so it should be hidden
        Unit unit =
            GameManager.Instantiate(Object, Vector3.zero, Quaternion.identity).GetComponent<Unit>();
        List<Unit> unitList = new List<Unit>(); unitList.Add(unit);


        //Add a listener what executes after players selects a position and returns it
        List<Vector2Int> positions = new List<Vector2Int>();

        if (ShouldCancel)
        {
            Debug.Log("PLAYERCREATE ACTION IS CANCELED");
            ShouldCancel = false;
            GameManager.Instance.HidePlacementEvent.Invoke(unitList);
            GameManager.Instance.CancelEvent.RemoveListener(Cancel);
            yield break;
        }

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

        //Start the action to select a position
        GameManager.Instance.ShowPlacementEvent.Invoke(unitList);
        GameManager.Instance.I_PositionSelect = Action_SelectPosition.Selecting(unit);
        yield return GameManager.Instance.StartCoroutine(GameManager.Instance.I_PositionSelect);
        GameManager.Instance.I_PositionSelect = null;

        //Waiting until we have the data
        while (IsWaitingForData && !ShouldCancel) { Debug.Log(ShouldCancel); yield return new WaitForSeconds(0.01f); }

            //Check if can place a unit there
            bool ViablePos = true;
            foreach (var v in positions)
            {
                if (!BoardManager.Instance.IsInBounds(v)) { ViablePos = false; break; }

                if (BoardManager.Instance.Board[v.x].Cells[v.y].CurUnit != null) { ViablePos = false; break; }
            }

            //Place a unit on said selected positions if they are viable
            GameManager.Instance.HidePlacementEvent.Invoke(unitList);
            if (ViablePos)
            {
                yield return GameManager.Instance.StartCoroutine(BoardManager.Instance.PlaceUnit(unit, positions));
            }
            else
            {
                Debug.Log("Non viable position");
            }



        ShouldCancel = false;
        GameManager.Instance.CancelEvent.RemoveListener(Cancel);
        yield return new WaitForSeconds(0.001f);
        GameManager.Instance.RemoveAction(parameters);
    }

}
