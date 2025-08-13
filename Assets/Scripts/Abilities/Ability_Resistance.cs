using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability_Resistance : Ability
{
    public Ability_Resistance(Ability_Name n, string d, Origin or, AbilityInstance i) : base(n, d, i)
    {
        aName = n;
        Description = d;
        Instance = i;
    }

    public DamageType Type;

}
