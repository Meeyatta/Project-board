using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Moveset", menuName ="Moveset")]
//Stores the relative coordinates of cells the unit can move to
public class Moveset : ScriptableObject
{
    public List<Vector2Int> Positions;
}
