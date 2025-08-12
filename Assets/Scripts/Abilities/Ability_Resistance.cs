using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability_Resistance : Ability
{
    public Ability_Resistance(Ability_Name n, string d, Origin or, Unit ow) : base(n, d, ow)
    {
        aName = n;
        Description = d;
        Owner = ow;
    }

    public DamageType Type;

}
