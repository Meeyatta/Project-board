using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Action_PlayerSpawn 
{

    public static IEnumerator PlayerSpawn(GameObject obj)
    {
        //Create a unit as an object, it's not on the board yet, so it should be hidden
        Unit unit =
            Object.Instantiate(obj, Vector3.zero, Quaternion.identity).GetComponent<Unit>();

        //Make it so we can receive a list of positions and save it
        bool Is_AwaitingData = true;
        List<Vector2Int> v = new List<Vector2Int>();
        void GetListOfPositions(List<Vector2Int> v2)
        {
            Is_AwaitingData = false;
            v = v2;
            SelectPosition.Instance.ESendPositionBack.RemoveListener(GetListOfPositions);
        }

        //Start the action to select a position
        List<Unit> unitList = new List<Unit>(); unitList.Add(unit);
        GameManager.Instance.ShowPlacementEvent.Invoke(unitList);
        SelectPosition.Instance.ESendPositionBack.AddListener(GetListOfPositions); //Add a listener what executes after players
                                                                                              //selects a position and returns it
        yield return GameManager.Instance.StartCoroutine(SelectPosition.Instance.Selecting(unit));

        //Waiting until we have the data, then hide the effects
        while (Is_AwaitingData) { Debug.Log("Awaiting data"); yield return new WaitForSeconds(0.1f); }
        GameManager.Instance.HidePlacementEvent.Invoke(unitList);

        //Place a unit on said selected position
        foreach (Vector2Int vv in v)
        {
            BoardManager.Instance.Board[vv.x].Cells[vv.y].CurUnit = unit;
        }
        unit.gameObject.transform.localPosition = BoardManager.Instance.Board[v[0].x].Cells[v[0].y].Position + unit.ModelOffset;

    }
}
