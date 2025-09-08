using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public static class Action_PlayerCreate
{
    static bool ShouldDebug = false;
    static string UnitPlacementHolderStr = "UnitPlacementHolder";
    public static Unit CurUnit;
    static GameObject UnitPlacementHolderObj;

    public static bool IsWaitingForData;

    public static bool IsViablePosition(Unit u, Vector2Int pos)
    {
        //Debug.Log("Checking if can create " + u.gameObject.name + " at " + pos);

        List<Vector2Int> uPos = BoardManager.Instance.SingleCellToUnitPositions(u, pos);

        List<Vector2Int> availableDeploymentsPosses = new List<Vector2Int>();
        int miX = BoardManager.Instance.PlayerDeployment_start.x; int maX = BoardManager.Instance.PlayerDeployment_end.x+1;
        int miY = BoardManager.Instance.PlayerDeployment_start.y; int maY = BoardManager.Instance.PlayerDeployment_end.y+1;
        for (int x = miX; x < maX; x++)
        {
            for (int y = miY; y < maY; y++)
            {
                if (BoardManager.Instance.Board[x].Cells[y].CurUnit == null) { availableDeploymentsPosses.Add(new Vector2Int(x, y)); }
            }
        }

        return uPos.Intersect<Vector2Int>(availableDeploymentsPosses).Any();
    }

    public static IEnumerator PlayerCreate(ActionParameters parameters)
    {
        GameObject Object = parameters.Object;

        UnitPlacementHolderObj = GameObject.Find(UnitPlacementHolderStr);
        Vector3 holdPos = Vector3.zero; if (UnitPlacementHolderObj != null) { holdPos = UnitPlacementHolderObj.transform.position; }

        #region Check if an object is already created or needs to be instantiated
        CurUnit = null;
        if (parameters.ActionTargetUnits != null && !parameters.ActionTargetUnits[0].IsPrefab)
        {
            yield return PlayerCreate(parameters.ActionTargetUnits[0]);
        }
        else
        {
            yield return PlayerCreate(parameters.Object);
        }
        #endregion

        yield return new WaitForSeconds(Time.deltaTime);
        GameManager.Instance.RemoveAction(parameters);
    }

    public static IEnumerator PlayerCreate(Unit unit)
    {
        UnitPlacementHolderObj = GameObject.Find(UnitPlacementHolderStr);
        Vector3 holdPos = Vector3.zero; if (UnitPlacementHolderObj != null) { holdPos = UnitPlacementHolderObj.transform.position; }

        CurUnit = unit;
        yield return Created_Placement(CurUnit);
    }

    public static IEnumerator PlayerCreate(GameObject gameobject)
    {
        UnitPlacementHolderObj = GameObject.Find(UnitPlacementHolderStr);
        Vector3 holdPos = Vector3.zero; if (UnitPlacementHolderObj != null) { holdPos = UnitPlacementHolderObj.transform.position; }

        CurUnit = GameManager.Instantiate(gameobject, holdPos, Quaternion.identity).GetComponent<Unit>();
        yield return Created_Placement(CurUnit);      
    }

    public static IEnumerator Created_Placement(Unit unit)
    {
        List<Unit> unitList = new List<Unit>(); unitList.Add(unit);

        //Add a listener what executes after players selects a position and returns it

        bool ViablePos = false;
        List<Vector2Int> positions = new List<Vector2Int>();
        void Get_ClickedCellCoordinates(List<Vector2Int> v2)
        {
            if (ShouldDebug) Debug.Log("Action_PlayerCreate Received ESendPositionBack");

            if (IsViablePosition(unitList[0], v2[0]))
            {
                ViablePos = true;
                IsWaitingForData = false;
                if (ShouldDebug) Debug.Log("Action_PlayerCreate - " + IsWaitingForData);
                positions = v2;
                Action_SelectUnitPosition.ESendPositionBack.RemoveListener(Get_ClickedCellCoordinates);
            }
            else
            {
                IsWaitingForData = true;
                if (ShouldDebug) Debug.Log("Action_PlayerCreate got data, but can't create in - " + v2[0]);
                GameManager.Instance.StartCoroutine(Action_SelectUnitPosition.Selecting_strict(CurUnit, false));
            }
        }

        Action_SelectUnitPosition.ESendPositionBack.RemoveListener(Get_ClickedCellCoordinates);
        Action_SelectUnitPosition.ESendPositionBack.AddListener(Get_ClickedCellCoordinates);
        IsWaitingForData = true;

        GameManager.Instance.StartCoroutine(Action_SelectUnitPosition.Selecting_strict(CurUnit, false));
        while (!ViablePos)
        {
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }

        #region Place a unit on said selected positions if they are viable
        GameManager.Instance.E_HidePlacement.Invoke(unitList);
        if (ViablePos)
        {
            //Debug.Log("Viable position, supposed to be creating");
            yield return CurUnit.OnFinishedCreation();
            yield return GameManager.Instance.StartCoroutine(BoardManager.Instance.PlaceUnit(CurUnit, positions));
        }
        else
        {
            if (ShouldDebug) Debug.Log("Non viable position");
        }
        #endregion
    }


}
