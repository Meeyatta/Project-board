using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Invincible")]
public class A_Invincible : Ability
{
    public A_Invincible(Ability_Name n, string d, AbilityInstance i) : base(n, d, i)
    {
        aName = n;
        Description = d;
        Instance = i;
    }
}
