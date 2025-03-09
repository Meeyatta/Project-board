using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Action_SelectUnit
{
    public static IEnumerator Select(ActionParameters parameters)
    {
        List<Vector2Int> CellsCoordinates = parameters.CellsCoordinates;
        if (CellsCoordinates.Count == 0) Debug.LogError("INVALID ACTION PARAMETERS - SELECT(CellCoordinates)");

       
        Vector2Int coords = CellsCoordinates[0];
        Debug.Log("SELECTED ON" + coords);

        
        Unit ogUnit = GameManager.Instance.CurUnitSelected; List<Unit> us = new List<Unit>();

        //I have absolutely no idea what this check was doing, but if it IS present it stops moveset appearing when selecting the same unit we already select
        if (true /*GameManager.Instance.CurUnitSelected != BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit*/)  
        {
            GameManager.Instance.CurUnitSelected = BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit;
            ogUnit = GameManager.Instance.CurUnitSelected;
            us.Add(GameManager.Instance.CurUnitSelected);

            //This is where we start to show current movement zones of the unit
            List<Unit> movable = new List<Unit>();
            foreach (var v in us) 
            { 

                if (Action_Move.UnitCanMove(v)) { Debug.Log(v + " didn't move yet, adding to the list"); movable.Add(v); } 
            }

            

            GameManager.Instance.ShowMovementEvent.Invoke(movable);      
        }


        yield return new WaitForSeconds(0.01f);
        while (GameManager.Instance.CurUnitSelected != null && ogUnit == GameManager.Instance.CurUnitSelected)
        {
            //Debug.Log("IS SELECTING A UNIT");
            yield return new WaitForSeconds(0.001f);
        }
        if (us.Count > 0) { GameManager.Instance.HideMovementEvent.Invoke(us); }
        GameManager.Instance.RemoveAction(parameters);
    }
}
