using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability_Entrenching : Ability
{
    public Ability_Entrenching(Ability_Name n, string d, Origin or, Unit ow) : base(n, d, ow)
    {
        aName = n;
        Description = d;
        Owner = ow;
    }

    public IEnumerator Try()
    {
        yield return null;
    }
}
