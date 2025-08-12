using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Base class for all abilities

[System.Serializable]
public class Ability
{
    public Ability_Name aName;
    public string Description;
    public Unit Owner;
    
    public Ability(Ability_Name n, string d, Unit ow)
    {
        aName = n;
        Description = d;
        Owner = ow;
    }
    public Ability() { }

}
