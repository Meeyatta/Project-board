using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.Windows;

[CreateAssetMenu(menuName = "Abilities/Score")]
public class A_Score : Ability
{
    public A_Score(Ability_Name n, string d, AbilityInstance i) : base(n, d, i)
    {
        aName = n;
        Description = d;
        Instance = i;
    }

    public string JitterAnimTrigger = "jitter";

    bool ShouldDebug = false;


    public int Check()
    {
        var l = BoardManager.Instance.Get_UnitsInRange(Instance.Owner, 1);
        if (ShouldDebug) foreach(var v in l) { Debug.Log("all: " + v.gameObject.name); }
        if (l == null || l.Count <= 0) return 0;

        if (ShouldDebug) Debug.Log("---");
        List<Unit> pU = GameManager.Instance.Get_OnlyUnitsWithKeywords(l, new List<Keyword> { Keyword.Player });
        if (ShouldDebug) foreach (var v in pU) { Debug.Log("player - " + v.gameObject.name); }

        List<Unit> eU = GameManager.Instance.Get_OnlyUnitsWithKeywords(l, new List<Keyword> { Keyword.Enemy });
        if (ShouldDebug) foreach (var v in eU) { Debug.Log("enemy - " + v.gameObject.name); }

        return pU.Count - eU.Count;
    }

    #region Conditions for either player or enemy scoring
    static bool PlayerScoreConditions(List<Unit> playerUnits, List<Unit> enemyUnits, List<Keyword> keywords) 
    {
        if (!keywords.Contains(Keyword.Player)) return false;

        return playerUnits != null && playerUnits.Count > 0 && (enemyUnits == null || enemyUnits.Count < playerUnits.Count);
    }
    static bool EnemyScoreConditions(List<Unit> playerUnits, List<Unit> enemyUnits, List<Keyword> keywords)
    {
        if (!keywords.Contains(Keyword.Enemy)) return false;

        return enemyUnits != null && enemyUnits.Count > 0 && (playerUnits == null || playerUnits.Count < enemyUnits.Count);
    }
    #endregion


}
