using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Was caused by another ability, persist until the unit dies or a specific moment 
public class Origin_Derivative : Origin_
{
    public Ability OriginalAbility; //What ability created this ability
    public Unit OrAbHolder;  //What unit had the OriginalAbility

    public Origin_Derivative(Ability a, Unit h)
    {
        OriginalAbility = a;
        OrAbHolder = h;
    }
}
