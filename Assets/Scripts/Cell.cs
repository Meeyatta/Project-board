using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CellTag { PlayerDeploymentZone, EnemyDeploymentZone }; // <- These tags will describe script-specific cell functions and AI directions
[System.Serializable]
public class Cell
{
    public Vector3 Position;
    public Vector2Int Coordinates; // Position on the grid
    public Unit CurUnit; // The object on top of this cell, if any

    public Covering CoveredBy;
    public List<CellTag> Tags = new List<CellTag>();
}
