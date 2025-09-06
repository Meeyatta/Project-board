using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Base class for all abilities

[System.Serializable]
public class Ability
{
    public Ability_Name aName;
    public string Description;
    public AbilityInstance Instance;
    
    public Ability(Ability_Name n, string d, AbilityInstance i)
    {
        aName = n;
        Description = d;
        Instance = i;
    }
    public Ability() { }

    public virtual void Start()
    {
        Debug.Log("Base ability Start in " + Instance.Owner.gameObject.name);
    }

    public virtual void Update()
    {

    }

}
