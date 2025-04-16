using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public static class Action_PlayerCreate
{
    static string UnitPlacementHolderStr = "UnitPlacementHolder";
    public static Unit Unit;
    static GameObject UnitPlacementHolderObj;

    public static bool IsWaitingForData;

    public static bool CanCreateThere(Unit u, Vector2Int pos)
    {
        Debug.Log("Checking if can create " + u.gameObject.name + " at " + pos);

        List<Vector2Int> uPos = BoardManager.Instance.SingleCellToUnitPositions(u, pos);

        List<Vector2Int> availableDeploymentsPosses = new List<Vector2Int>();
        int miX = BoardManager.Instance.PlayerDeploymentZone[0].x; int maX = BoardManager.Instance.PlayerDeploymentZone[1].x; 
        int miY = BoardManager.Instance.PlayerDeploymentZone[0].y; int maY = BoardManager.Instance.PlayerDeploymentZone[1].y;
        for (int x = miX; x < maX; x++) 
        {
            for (int y = miY; y < maY; y++) 
            {
                if (BoardManager.Instance.Board[x].Cells[y].CurUnit == null) { availableDeploymentsPosses.Add(new Vector2Int(x, y));}
            }
        }

        return uPos.Intersect<Vector2Int>(availableDeploymentsPosses).Any();
    }

    public static IEnumerator PlayerCreate(ActionParameters parameters)
    {

        GameObject Object = parameters.Object;
        List<Vector2Int> zone = parameters.CellsCoordinates;
   
        UnitPlacementHolderObj = GameObject.Find(UnitPlacementHolderStr);
        Vector3 holdPos = Vector3.zero; if (UnitPlacementHolderObj != null) { holdPos = UnitPlacementHolderObj.transform.position; }

        #region Check if an object is already created or needs to be instantiated
        Unit = null;
        if (parameters.ActionTargetUnits != null && !parameters.ActionTargetUnits[0].IsPrefab)
        {
            Unit = parameters.ActionTargetUnits[0];
        }
        else
        {
            Unit = GameManager.Instantiate(Object, holdPos, Quaternion.identity).GetComponent<Unit>();
        }
        #endregion

        List<Unit> unitList = new List<Unit>(); unitList.Add(Unit);

        //Add a listener what executes after players selects a position and returns it
        List<Vector2Int> positions = new List<Vector2Int>();
        void Get_ClickedCellCoordinates(List<Vector2Int> v2)
        {
            if (CanCreateThere(unitList[0], v2[0]))
            {
                IsWaitingForData = false;
                positions = v2;
                Action_SelectPosition.ESendPositionBack.RemoveListener(Get_ClickedCellCoordinates);
            }        
        }

        UnityEvent<List<Vector2Int>> newEv = new UnityEngine.Events.UnityEvent<List<Vector2Int>>() { };
        Action_SelectPosition.ESendPositionBack = newEv;
        Action_SelectPosition.ESendPositionBack.AddListener(Get_ClickedCellCoordinates);
        IsWaitingForData = true;

        #region Start the action to select a position
        GameManager.Instance.ShowPlacementEvent.Invoke(unitList);
        GameManager.Instance.I_PositionSelect = Action_SelectPosition.Selecting(Unit, false);
        yield return GameManager.Instance.StartCoroutine(GameManager.Instance.I_PositionSelect);
        GameManager.Instance.I_PositionSelect = null;
        #endregion

       // GameManager.Instance.ShowDeploymentEvent.Invoke(new List<Unit> { Unit });

        //Waiting until we have the data

        while (IsWaitingForData) { yield return new WaitForSeconds(Time.deltaTime * 0.5f); }

        Debug.Log("Got past waiting for data");

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
            Debug.Log("Viable position, supposed to be creating");
            yield return GameManager.Instance.StartCoroutine(BoardManager.Instance.PlaceUnit(Unit, positions));
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
