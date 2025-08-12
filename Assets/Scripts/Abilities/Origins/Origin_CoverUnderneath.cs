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
        Debug.Log("Checking if Origin_CoverUnderneath of " + Parent.Ability_.aName + " is equal to" + o.GetType() + " of " + o.Parent.Ability_.aName);
        if (o is Origin_CoverUnderneath origin)
        {
            Debug.Log("Stats of this: " + CellCoord + " " + CoverType);
            Debug.Log("Stats of compared to: " + origin.CellCoord + " " + origin.CoverType);
            return (origin.CellCoord == CellCoord && origin.CoverType == CoverType);
        }
        else
        {
            Debug.Log("Compared to origin is NOT cover underneath, origin is " + o.GetType());
        }
        return false;
    }
}
