using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A_Entrenching : Ability
{
    public A_Entrenching(Ability_Name n, string d, AbilityInstance i) : base(n, d, i)
    {
        aName = n;
        Description = d;
        Instance = i;
    }

    public override void Start()
    {
        BattleStatsManager.Instance.E_RoundStart.RemoveListener(Apply);
        BattleStatsManager.Instance.E_RoundStart.AddListener(Apply);
    }

    public override void Update()
    {
        if (Instance.Owner.Moved)
        {
            Debug.Log("Moved, trying to remove the resistance");

            if (appliedInstance != null) Instance.Owner.Ability_Remove(appliedInstance);
            else { Debug.LogError("Unit doesn't have the resistance"); }
        }
    }

    //This should be called at the start of each round
    AbilityInstance appliedInstance;
    public void Apply(int r)
    {
        Debug.Log("Apply " + Instance.Owner.gameObject.name + " - " + Instance.Owner.Moved);

        if (!Instance.Owner.Moved)
        {
            Origin_Derivative origin = new Origin_Derivative(null, Ability_Name.Entrenching, Instance.Owner);
            AbilityInstance abilityInstance = AbilityManager.Instance.Ability_Create(Instance.Owner, Ability_Name.Resistance_Physical, origin);
            if (origin != null) origin.Parent = abilityInstance;

            appliedInstance = abilityInstance;
            Instance.Owner.Ability_Add(appliedInstance);
        }
        else
        {

        }
    }
}
