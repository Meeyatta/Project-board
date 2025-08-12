using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Was caused by another ability, persist until the unit dies or a specific moment 
[System.Serializable]
public class Origin_Derivative : Origin
{
    public Ability_Name OriginalAbility; //What ability created this ability
    public Unit OrAbHolder;  //What unit had the OriginalAbility

    public Origin_Derivative(AbilityInstance p ,Ability_Name n, Unit h) : base(p)
    {
        Parent = p;
        OriginalAbility = n;
        OrAbHolder = h;
    }

    public override bool Equals(Origin o)
    {
        if (o is Origin_Derivative origin)
        {
            return (origin.OriginalAbility == OriginalAbility && origin.OrAbHolder == OrAbHolder);
        }

        return false;
    }
}
