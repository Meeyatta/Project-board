using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActionParameters
{
    public GameManager.ActionType Type;
    public List<Unit> ActionTargetUnits;
    public List<Unit.Keyword> Keywords;
    public List<Vector2Int> CellsCoordinates;
    public GameObject Object;

    public ActionParameters(GameManager.ActionType type, List<Unit> actionTargetUnits, List<Unit.Keyword> keywords, List<Vector2Int> cellsCoordinates, GameObject object_)
    {
        this.Type = type;
        this.ActionTargetUnits = actionTargetUnits;
        this.Keywords = keywords;
        this.CellsCoordinates = cellsCoordinates;
        this.Object = object_;
    }

}
