using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A_Entrenching : Ability
{
    public A_Entrenching(Ability_Name n, string d, AbilityInstance i) : base(n, d, i)
    {
        aName = n;
        Description = d;
        Instance = i;
    }

    public IEnumerator Try()
    {
        yield return null;
    }
}
