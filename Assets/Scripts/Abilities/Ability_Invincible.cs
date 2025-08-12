using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Invincible")]
public class Ability_Invincible : Ability
{
    public Ability_Invincible(Ability_Name n, string d, Origin or, Unit ow) : base(n, d, ow)
    {
        aName = n;
        Description = d;
        Owner = ow;
    }
}
