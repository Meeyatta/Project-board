using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class Action_Create
{
    public static bool ShouldDebug = false;
    public static IEnumerator Create(ActionParameters parameters)
    {
        GameObject Object = parameters.Object; List<Vector2Int> poss = parameters.CellsCoordinates;
        int side = parameters.IntNumber;

        #region Checking if we need to instantiate a new unit or if we can use an old one
        Unit unit = new Unit();
        if (parameters.ActionTargetUnits != null && parameters.ActionTargetUnits.Count > 0) 
        {
            unit = parameters.ActionTargetUnits[0]; 
        }
        else
        {
            unit = GameManager.Instantiate(Object, Vector3.zero, Quaternion.identity).GetComponent<Unit>();
        }
        #endregion

        if (ShouldDebug) { Debug.Log("Creating a" + unit.gameObject.name + " as team: " + side); }

        #region Setting unit to one of the sides - player, enemy or neither
        switch (side) 
        {
            case -1:
                unit.SetToNeutral();
                break;
            case 0:
                unit.SetToPlayer();
                break;
            default:
                unit.SetToEnemy();
                break;
        }
        #endregion

        bool ViablePos = true;
        foreach (var v in poss)
        {
            if (!BoardManager.Instance.IsInBounds(v)) { ViablePos = false; Debug.Log(v + " is out of bounds"); break; }

            if (BoardManager.Instance.Board[v.x].Cells[v.y].CurUnit != null) 
            { ViablePos = false; Debug.Log(v + " is occupied by " + BoardManager.Instance.Board[v.x].Cells[v.y].CurUnit.gameObject.name); break; }
        }

        //Debug.Log("ViablePos is: " + ViablePos);
        if (ViablePos)
        {
            yield return GameManager.Instance.StartCoroutine(BoardManager.Instance.PlaceUnit(unit, poss));
        }
        else
        {
            Debug.Log("Non viable position");
        }

        yield return new WaitForSeconds(Time.fixedDeltaTime);
        GameManager.Instance.RemoveAction(parameters);
    }
}
