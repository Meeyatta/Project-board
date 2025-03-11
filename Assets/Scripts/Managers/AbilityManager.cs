using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Continuously checks if conditions for specific unit ablities are satisfied, if so, triggers them

public enum Ability { 
    Score,              //Adds score to player/enemy side if their units are within objectives
    Invincible,       //All incoming damage is set to 0
    Fireproof,        //-1 incoming fire damage
    Flammable,        //+1 incoming fire damage
    Conductive,       //+1 incoming lightning damage
    Nonconductive,    //-1 incoming lightning damage
    Armored,          //-1 incoming physical damage
    Exposed,          //+1 incoming physical damage
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


}
