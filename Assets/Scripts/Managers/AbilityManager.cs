using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

//Holds all of the abilities and is responsible for things with them

public enum Ability_Name 
{ 
    Invincible,       //All incoming damage is set to 0
    Vulnerability,    //Incoming damage of a specific type is increased by 1
    Resistance,       //Incoming damage of a specific type is decreased by 1
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

    #region ModifyDamage involves functionality of all abilities what modify incoming damage. 
    public int ModifyIncomingDamage(Unit target, int starterDamage, DamageType damageType)
    {
        int endDamage = starterDamage;

        #region Decreasing the damage in case of resistances
        List<Ability> resistances = target.Get_Abilities_ByName(Ability_Name.Resistance);
        if (resistances != null)
        {
            Debug.Log("Got " + resistances.Count + " resistances for " + target.UnitName);
            foreach (var r in resistances)
            {
                Ability_Resistance ar = r as Ability_Resistance;
                if (ar != null)
                {
                    Debug.Log("Got resistance: " + ar.Type + " attack type: " + damageType);
                    if (ar.Type == damageType)
                    {
                        endDamage = Mathf.Clamp(endDamage - 1, 0, endDamage);
                        Debug.Log("Decreasing the " + damageType + " damage to " + endDamage);
                    }
                }
                else
                {
                    Debug.Log("Can't convert " + r.Name + " to resistance");
                }
            }
        }  
        #endregion

        #region Increasing the damage in case of vulnerabilities
        List<Ability> vulnerabilities = target.Get_Abilities_ByName(Ability_Name.Vulnerability);
        if (vulnerabilities != null)
        {
            Debug.Log("Got " + vulnerabilities.Count + " vulnerabilities for " + target.UnitName);

            foreach (var v in vulnerabilities)
            {
                Ability_Vulnerability av = v as Ability_Vulnerability;
                Debug.Log("Got vulnerability: " + av.Type + " attack type: " + damageType);
                if (av != null)
                {
                    if (av.Type == damageType)
                    {
                        endDamage = Mathf.Clamp(endDamage + 1, endDamage, 99999);
                        Debug.Log("Increasing the " + damageType + " damage to " + endDamage);
                    }
                }
                else
                {
                    Debug.Log("Can't convert " + v.Name + " to vulnerability");
                }
            }
        }
        #endregion

        if (target.HasAbility(Ability_Name.Invincible) || endDamage < 0) { endDamage = 0; }

        return endDamage;
    }
    #endregion

}
