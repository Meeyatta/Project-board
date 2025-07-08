using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

//Holds all of the abilities and is responsible for things with them

public enum Ability_Name 
{ 
    Invincible,       //All incoming damage is set to 0
    Resistant_Fire,        //-1 incoming fire damage
    Vulnerable_Fire,        //+1 incoming fire damage
    Vulnerable_Lightning,       //+1 incoming lightning damage
    Resistant_Lightning,    //-1 incoming lightning damage
    Resistant_Physical,          //-1 incoming physical damage
    Vulnerable_Physical,          //+1 incoming physical damage
    Resistant_Poison,         //-1 incoming poison damage
    Vulnerable_Poison,           //+1 incoming poison damage
    Resistant_Frost,        //-1 incoming frost damage
    Vulnerable_Frost,              //+1 incoming frost damage

    Score,              //Adds score to player/enemy side if their units are within objectives
    Slippery,         //After ending their move, unit moves 1 cell in a random direction

}



public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance;
    Coroutine CurCheckForAbilities = null;
    void Singleton()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    void Awake()
    {
        Instance = this;
    }

    #region ModifyDamage involves functionality of all abilities what modify incoming damage. This monstrocity has to be changed, but so far I need it to be easily read, not efficient
    public int ModifyIncomingDamage(Unit target, int starterDamage)
    {
        int endDamage = starterDamage;
        DamageType type = target.CurAttackZone.Type;

        #region Modifying damage depending on abilities
        switch (type)
        {
            case DamageType.Fire:
                if (target.HasAbility(Ability_Name.Resistant_Fire)) { endDamage -= 1; }
                if (target.HasAbility(Ability_Name.Vulnerable_Fire)) { endDamage += 1; }
                break;
            case DamageType.Lightning:
                if (target.HasAbility(Ability_Name.Resistant_Lightning)) { endDamage -= 1; }
                if (target.HasAbility(Ability_Name.Vulnerable_Lightning)) { endDamage += 1; }
                break;
            case DamageType.Physical:
                if (target.HasAbility(Ability_Name.Resistant_Physical)) { endDamage -= 1; }
                if (target.HasAbility(Ability_Name.Vulnerable_Physical)) { endDamage += 1; }
                break;
            case DamageType.Poison:
                if (target.HasAbility(Ability_Name.Resistant_Poison)) { endDamage -= 1; }
                if (target.HasAbility(Ability_Name.Vulnerable_Poison)) { endDamage += 1; }
                break;
            case DamageType.Frost:
                if (target.HasAbility(Ability_Name.Resistant_Frost)) { endDamage -= 1; }
                if (target.HasAbility(Ability_Name.Vulnerable_Frost)) { endDamage += 1; }
                break;
        }
        #endregion
        if (target.HasAbility(Ability_Name.Invincible) || endDamage < 0) { endDamage = 0; }

        return endDamage;
    }
    #endregion

}
