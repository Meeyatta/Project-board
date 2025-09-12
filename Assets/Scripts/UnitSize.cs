using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Size", menuName = "Unit Size")]
/*
    Stores the coordinates the unit takes up. Must have at least a singular (0,0) coordinate,
        all other coordinates are relative to that one
*/

/*
    VARIABLES:
        Positions - Positions of all cells relative to the most top-left position of the unit
 */

public class UnitSize : ScriptableObject
{
    public List<Vector2Int> Positions;
}
