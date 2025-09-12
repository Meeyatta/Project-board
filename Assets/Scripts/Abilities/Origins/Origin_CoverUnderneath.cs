using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Unit has this ability as long as it is standing on a specific cover (Water, Oil, etc...)
[System.Serializable]
public class Origin_CoverUnderneath : Origin
{
    public Vector2Int CellCoord; //Coordinates of the covered cell
    public CoverType CoverType; //Cover type on that cell

    public Origin_CoverUnderneath (AbilityInstance p,Vector2Int cc, CoverType ct) : base(p)
    {
        Parent = p;
        CellCoord = cc;
        CoverType = ct;
    }

    public override bool Equals(Origin o)
    {
        if (o is Origin_CoverUnderneath origin)
        {         
            return (origin.CellCoord == CellCoord && origin.CoverType == CoverType);
        }
        else
        {
            
        }
        return false;
    }
}
