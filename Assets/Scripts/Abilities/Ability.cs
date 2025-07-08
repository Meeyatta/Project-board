using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Base class for all abilities

[System.Serializable]
public class Ability : ScriptableObject
{
    public Ability_Name Name;
    public string Description;
    public Origin Origin_;
    public Unit Owner;
    
    public Ability(Ability_Name n, string d, Origin or, Unit ow)
    {
        Name = n;
        Description = d;
        Origin_ = or;
        Owner = ow;
    }

    public virtual void Remove()
    {
        if (Owner != null)
        {
            Owner.AbilityRemove(this);
        }
    }
}
