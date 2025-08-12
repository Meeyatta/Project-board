using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Origin_Innate : Origin
{
    public Origin_Innate(AbilityInstance a) : base(a)
    {
        Parent = a;
    }

    //Unit starts with this ability by default and always has it during battle, unless it is explicitly removed
    public override bool Equals(Origin o)
    {
        return (o.GetType() == this.GetType());
    }
}
