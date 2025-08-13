using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

//Holds all of the abilities and is responsible for things with them

public enum Ability_Name 
{ 
    Invincible,       //All incoming damage is set to 0

    #region Resistances - Incoming damage of that type is reduced by 1
    Resistance_Fire,
    Resistance_Lightning,
    Resistance_Frost,

    #endregion

    #region Vulnerabilities - Incoming damage of that type is increased by 1
    Vulnerable_Fire,
    Vulnerable_Lightning,
    Vulnerable_Frost,
    #endregion

    Score,              //Adds score to player/enemy side if their units are within objectives
    Slippery,         //After ending their move, unit moves 1 cell in a random direction
    Entrenching         //At the beginning of the round, if the unit hasn't moved on their previous turn, gains "Resistance:Physical" until moves
}



public class AbilityManager : MonoBehaviour
{
    public bool ShouldDebug;
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

    //Quickly adding blank abilities by their name
    public AbilityInstance Ability_Create(Unit target, Ability_Name name, Origin origin)
    {
        AbilityInstance abilityInstance = new AbilityInstance(null, origin, target);
        origin.Parent = abilityInstance;

        Ability ability = null;

        switch (name)
        {
            case Ability_Name.Invincible:
                ability = new A_Invincible(name, "", abilityInstance);
                break;

            #region Resistances
            case Ability_Name.Resistance_Fire:
                ability = new A_Resistance(name,DamageType.Fire, "", abilityInstance);
                break;
            case Ability_Name.Resistance_Lightning:
                ability = new A_Resistance(name, DamageType.Lightning, "", abilityInstance);
                break;
            case Ability_Name.Resistance_Frost:
                ability = new A_Resistance(name, DamageType.Frost, "", abilityInstance);
                break;
            #endregion

            #region Vulnerabilities
            case Ability_Name.Vulnerable_Fire:
                ability = new A_Vulnerability(name, DamageType.Fire, "", abilityInstance);
                break;
            case Ability_Name.Vulnerable_Lightning:
                ability = new A_Vulnerability(name, DamageType.Lightning, "", abilityInstance);
                break;
            case Ability_Name.Vulnerable_Frost:
                ability = new A_Vulnerability(name, DamageType.Frost, "", abilityInstance);
                break;
            #endregion

            case Ability_Name.Slippery:
                ability = new A_Slippery(name, "", abilityInstance);
                break;

            case Ability_Name.Entrenching:
                ability = new A_Entrenching(name, "", abilityInstance);
                break;

            case Ability_Name.Score:
                ability = new A_Score(name, "", abilityInstance);
                break;

            default:
                Debug.LogError(name + " abiltiy is not set up for AbilityManager");
                break;
        }

        abilityInstance.Ability_ = ability;
        return abilityInstance;
    }

    #region ModifyDamage involves functionality of all abilities what modify incoming damage. 
    public int ModifyIncomingDamage(Unit target, int starterDamage, DamageType damageType)
    {
        int endDamage = starterDamage;

        #region Decreasing the damage in case of resistances
        List<AbilityInstance> resistances = new List<AbilityInstance>();
        foreach (var ab in target.Abilities)
        {
            if (ab.Ability_ is A_Resistance) { resistances.Add(ab); }
        }

        if (resistances != null && resistances.Count > 0)
        {
            if (ShouldDebug) Debug.Log("Got " + resistances.Count + " resistances for " + target.UnitName);
            foreach (var r in resistances)
            {
                Ability_Resistance ar = r.Ability_ as Ability_Resistance;
                if (ar == null) { continue; }

                if (ShouldDebug) Debug.Log("Got resistance: " + ar.Type + " attack type: " + damageType);
                if (ar.Type == damageType)
                {
                    endDamage = Mathf.Clamp(endDamage - 1, 0, endDamage);
                    if (ShouldDebug) Debug.Log("Decreasing the " + damageType + " damage to " + endDamage);
                }
            }
        }  
        #endregion

        #region Increasing the damage in case of vulnerabilities
        List<AbilityInstance> vulnerabilities = new List<AbilityInstance>();
        foreach (var ab in target.Abilities)
        {
            if (ab.Ability_ is A_Vulnerability) { vulnerabilities.Add(ab); }
        }

        if (vulnerabilities != null)
        {
            if (ShouldDebug) Debug.Log("Got " + vulnerabilities.Count + " vulnerabilities for " + target.UnitName);

            foreach (var v in vulnerabilities)
            {
                Ability_Vulnerability av = v.Ability_ as Ability_Vulnerability;
                if (ShouldDebug) Debug.Log("Got vulnerability: " + av.Type + " attack type: " + damageType);
                if (av != null)
                {
                    if (av.Type == damageType)
                    {
                        endDamage = Mathf.Clamp(endDamage + 1, endDamage, 99999);
                        if (ShouldDebug) Debug.Log("Increasing the " + damageType + " damage to " + endDamage);
                    }
                }
                else
                {
                    if (ShouldDebug) Debug.Log("Can't convert " + v.Ability_.aName + " to vulnerability");
                }
            }
        }
        #endregion

        if (target.HasAbility(Ability_Name.Invincible) || endDamage < 0) { endDamage = 0; }

        return endDamage;
    }
    #endregion

}
