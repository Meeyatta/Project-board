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
    
        GameManager.Instance.CurUnitSelected = BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit;
        ogUnit = GameManager.Instance.CurUnitSelected;
        us.Add(GameManager.Instance.CurUnitSelected);


        #region If current turn is player turn
        if (ScoreManager.Instance.PlayerTurnActionCondition())
        {
            #region If round is not deployment - show unit's movement
            if (ScoreManager.Instance.CurRound > 0)
            {
                #region Create a list of all units whose movement needs to be shown and call an event to start doing so
                List<Unit> movable = new List<Unit>();
                foreach (var v in us)
                {
                    if (Action_Move.UnitCanMove(v)) { /*Debug.Log(v + " didn't move yet, adding to the list");*/ movable.Add(v); }
                }

                GameManager.Instance.ShowMovementEvent.Invoke(movable);
                #endregion
            }
            #endregion
            #region If round is deployment ( 0 round ) - show available deployment positions
            else
            {
                List<Unit> redeploy = new List<Unit>();
                foreach (var v in us)
                {
                    if (Action_Move.UnitCanMove(v)) { redeploy.Add(v); }
                }

                GameManager.Instance.ShowDeploymentEvent.Invoke(redeploy);

            }
            #endregion

            AudioManager.Instance.Play(SoundName.Click, GameManager.Instance.CurUnitSelected.transform);
            yield return new WaitForSeconds(Time.deltaTime);

            #region Wait shile we are selecting the unit
            while (GameManager.Instance.CurUnitSelected != null && ogUnit == GameManager.Instance.CurUnitSelected)
            {
                //Debug.Log("IS SELECTING A UNIT");
                yield return new WaitForSeconds(Time.deltaTime);
            }
            #endregion

            if (us.Count > 0) { GameManager.Instance.HideMovementEvent.Invoke(us); } //Event to hide movement effects of selected units
            if (us.Count > 0) { GameManager.Instance.HideDeploymentEvent.Invoke(us); } //Event to hide deployment effects of selected units


            GameManager.Instance.RemoveAction(parameters);
            yield break;
        }
        #endregion

        #region If current turn is enemy turn
        else
        {
            //TODO:

            GameManager.Instance.RemoveAction(parameters);
            yield break;
        }
        #endregion
    }
}
