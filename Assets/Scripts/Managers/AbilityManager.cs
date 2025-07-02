using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

//Continuously checks if conditions for specific unit ablities are satisfied, if so, triggers them

public enum Ability 
{ 
    Invincible,       //All incoming damage is set to 0
    Resistant_Fire,        //-1 incoming fire damage
    Vulnerable_Fire,        //+1 incoming fire damage
    Vulnerable_Lightning,       //+1 incoming lightning damage
    Resistant_Lightning,    //-1 incoming lightning damage
    Resistant_Physical,          //-1 incoming physical damage
    Vulnerable_Physical,          //+1 incoming physical damage
    Endruing,         //-1 incoming poison damage
    Sickly,           //+1 incoming poison damage
    Insulated,        //-1 incoming frost damage
    Wet,              //+1 incoming frost damage

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

    #region ModifyDamage involves functionality of all abilities what modify incoming damage
    public int ModifyIncomingDamage(Unit target, int starterDamage)
    {
        int endDamage = starterDamage;
        DamageType type = target.CurAttackZone.Type;

        #region Modifying damage depending on abilities
        switch (type)
        {
            case DamageType.Fire:
                if (target.CurAbilities.Contains(Ability.Resistant_Fire)) { endDamage -= 1; }
                if (target.CurAbilities.Contains(Ability.Vulnerable_Fire)) { endDamage += 1; }
                break;
            case DamageType.Lightning:
                if (target.CurAbilities.Contains(Ability.Resistant_Lightning)) { endDamage -= 1; }
                if (target.CurAbilities.Contains(Ability.Vulnerable_Lightning)) { endDamage += 1; }
                break;
            case DamageType.Physical:
                if (target.CurAbilities.Contains(Ability.Resistant_Physical)) { endDamage -= 1; }
                if (target.CurAbilities.Contains(Ability.Vulnerable_Physical)) { endDamage += 1; }
                break;
            case DamageType.Poison:
                if (target.CurAbilities.Contains(Ability.Endruing)) { endDamage -= 1; }
                if (target.CurAbilities.Contains(Ability.Sickly)) { endDamage += 1; }
                break;
            case DamageType.Frost:
                if (target.CurAbilities.Contains(Ability.Insulated)) { endDamage -= 1; }
                if (target.CurAbilities.Contains(Ability.Wet)) { endDamage += 1; }
                break;
        }
        #endregion
        if (target.CurAbilities.Contains(Ability.Invincible) || endDamage < 0) { endDamage = 0; }

        return endDamage;
    }
    #endregion

}
