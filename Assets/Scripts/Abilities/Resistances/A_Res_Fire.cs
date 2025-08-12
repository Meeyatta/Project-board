using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class A_Res_Fire : Ability
{
    public A_Res_Fire(Ability_Name n, string d, Origin or, Unit ow) : base(n, d, ow)
    {
        aName = n;
        Description = d;
        Owner = ow;
    }

}
