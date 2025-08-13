using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A_Resistance : Ability
{
    public DamageType Type;
    public A_Resistance(Ability_Name n, DamageType type, string d, AbilityInstance i) : base(n, d, i)
    {
        aName = n;
        Description = d;
        Instance = i;
    }
}
