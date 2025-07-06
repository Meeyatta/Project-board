using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OriginType  
{
    Innate, //Unit starts with this ability by default and always has it during battle, unless it is explicitly removed
    CoverUnderneath, //Unit has this ability as long as it is standing on a specific cover (Water, Oil, etc...)
    Derivative,  //Was caused by another ability, persist until the unit dies or a specific moment 
}

//This shows what kind of source the ability has: Is it innate, is it from an underneath cell cover
[System.Serializable]
public class AbilitySource 
{
    public Origin Origin;

    public AbilitySource(OriginType t, Origin o)
    {

    }
}
