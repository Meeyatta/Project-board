using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Resistance")]
public class Ability_Resistance : Ability
{
    public Ability_Resistance(Ability_Name n, string d, Origin or, Unit ow) : base(n, d, or, ow)
    {
        Name = n;
        Description = d;
        Origin_ = or;
        Owner = ow;
    }

    public DamageType Type;
}
