using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Moveset", menuName ="Moveset")]
//Stores the relative coordinates of cells the unit can move to
public class Moveset : ScriptableObject
{
    [System.Serializable]
    public class Line{
        public string Direction;
        public List<Vector2Int> Positions;
    }

    public bool IsEvading;
    public List<Line> Lines;
}
