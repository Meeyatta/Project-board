using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Cell
{
    public Vector3 Position;
    public Vector2Int Coordinates; // Position on the grid
    public Unit CurUnit; // The object on top of this cell, if any
}
