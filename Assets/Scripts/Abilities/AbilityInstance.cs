using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AbilityInstance
{
    public Ability Ability_;
    public Origin Origin_;
    public Unit Owner;

    public AbilityInstance(Ability ab, Origin or, Unit ow)
    {
        Ability_ = ab;
        Origin_ = or;
        Owner = ow;
    }
}
