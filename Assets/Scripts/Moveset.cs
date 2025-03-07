using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Moveset", menuName = "Moveset")]
//Stores the relative coordinates of cells the unit can move to

/*
    VARIABLES:
        Direction - For testing and convenience, edit in inspector to show what the direction this line has
        Positions - Positions of all cells relative to the unit
        IsEvading - Can unit "jump over" other units when moving along this line?
 */

public class Moveset : ScriptableObject
{
    [System.Serializable]
    public class Cells
    {
        public List<Vector2Int> Cords;
    }

    [System.Serializable]
    public class Line{
        public string Name;
        public List<Cells> Positions;
    }

    public bool IsEvading;
    public List<Line> Lines;
}
