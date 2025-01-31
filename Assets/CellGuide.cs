using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New CellGuide", menuName = "CellGuide")]
//Stores the relative coordinates of cells the unit can move to

/*
    VARIABLES:
        Direction - For testing and convenience, edit in inspector to show what the direction this line has
        Positions - Positions of all cells relative to the unit
        IsEvading - Can unit "jump over" other units when moving along this line?
 */

public class CellGuide : ScriptableObject
{
    [System.Serializable]
    public class Line{
        public string Direction;
        public List<Vector2Int> Positions;
        public bool IsEvading;
    }

    public bool IsEvading;
    public List<Line> Lines;
}
