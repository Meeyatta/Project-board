using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActionParameters
{
    public GameManager.ActionType Type;
    public List<Unit> ActionTargetUnits;
    public List<Keyword> Keywords;
    public List<Vector2Int> CellsCoordinates;
    public int IntNumber;

    public ActionParameters(
        GameManager.ActionType type, 
        List<Unit> actionTargetUnits,
        List<Keyword> keywords,
        List<Vector2Int> cellsCoordinates,
        int intNumber)
    {
        this.Type = type;
        this.ActionTargetUnits = actionTargetUnits;
        this.Keywords = keywords;
        this.CellsCoordinates = cellsCoordinates;
        this.IntNumber = intNumber;
    }

}
