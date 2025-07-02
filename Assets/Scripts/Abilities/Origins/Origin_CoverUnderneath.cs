using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Unit has this ability as long as it is standing on a specific cover (Water, Oil, etc...)
public class Origin_CoverUnderneath : Origin_
{
    public Vector2Int CellCoord; //Coordinates of the covered cell
    public CoverType CoverType; //Cover type on that cell

    public Origin_CoverUnderneath (Vector2Int cc, CoverType ct)
    {
        CellCoord = cc;
        CoverType = ct;
    }
}
