using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Origin 
{
    /*
        Types of ability Origins:
    Origin_Innate - Unit starts with this ability by default and always has it during battle, unless it is explicitly removed
    Origin_CoverUnderneath - Unit has this ability as long as it is standing on a specific cover (Water, Oil, etc...)
    Origin_Derivative - Was caused by another ability, persist until the unit dies or a specific moment 

    */

    //Function unique to each origin which checks if an origin is equal to another one
    public virtual bool Equals(Origin o)
    {
        return (o.GetType() == this.GetType());
    }
}
