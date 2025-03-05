using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.Windows;

public static class Ability_Score 
{
    //Checks individual objective unit if it can score for player/enemy
    public static bool Check(Unit u, List<Unit.Keyword> keywords)
    {
        if (!u.CurAbilities.Contains(Unit.Ability.Score)) return false;

        //TODO: Add a check if it IS an objective, but doesn't need scoring
        var l = BoardManager.Instance.Get_UnitsInRange(u, 1);
        if (l == null || l.Count <= 0) return false;


        List<Unit> totl = new List<Unit>();
        foreach ( var k in keywords )
        {
            List<Unit.Keyword> keyw = new List<Unit.Keyword> { k };
            totl.AddRange(GameManager.Instance.Get_OnlyUnitsWithKeywords(l, keyw));
        }

        if (totl == null || totl.Count <= 0) return false;

        return true;

    }
    //Main function, goes through every objective on the map and scores appropriately (checks for the keywords of surrounding units)
    public static IEnumerator GlobalScore(List<Unit.Keyword> keywords)
    {
        yield return new WaitForSeconds(0.01f);

        List<Unit.Keyword> ks = new List<Unit.Keyword> { Unit.Keyword.Objective };
        List<Unit> all = BoardManager.Instance.Get_AllUnitsWithKeywords(ks);

        List<Unit> objectives = new List<Unit>();
        foreach (var a in all)
        {
            if (Check(a, keywords))
            {
                objectives.Add(a);
            }
        }
        yield return BoardManager.Instance.StartCoroutine(Score(objectives, keywords));

        yield return new WaitForSeconds(0.01f);
    }
    #region Conditions for either player or enemy scoring
    static bool PlayerScoreConditions(List<Unit> nPU, List<Unit> nEU, List<Unit.Keyword> keywords) 
    {
        if (!keywords.Contains(Unit.Keyword.Player)) return false;

        return nPU != null && nPU.Count > 0 && (nEU == null || nEU.Count <= 0);
    }
    static bool EnemyScoreConditions(List<Unit> nPU, List<Unit> nEU, List<Unit.Keyword> keywords)
    {
        if (!keywords.Contains(Unit.Keyword.Enemy)) return false;

        return nEU != null && nEU.Count > 0 && (nPU == null || nPU.Count <= 0);
    }
    #endregion

    //Scoring for an individual objective unit
    public static IEnumerator Score(List<Unit> ActionTargetUnits, List<Unit.Keyword> keywords)
    {
        yield return new WaitForSeconds(0.01f);

        foreach (var u in ActionTargetUnits)
        {
            var l = BoardManager.Instance.Get_UnitsInRange(u, 1);

            List<Unit.Keyword> pk = new List<Unit.Keyword> { Unit.Keyword.Player };
            List<Unit> pu = GameManager.Instance.Get_OnlyUnitsWithKeywords(l, pk);

            List<Unit.Keyword> ek = new List<Unit.Keyword> { Unit.Keyword.Enemy };
            List<Unit> eu = GameManager.Instance.Get_OnlyUnitsWithKeywords(l, ek);

            if (PlayerScoreConditions(pu, eu, keywords)) { ScoreManager.Instance.Score_Player++; }

            if (EnemyScoreConditions(pu, eu, keywords)) { ScoreManager.Instance.Score_Enemy++; }

        }
        yield return new WaitForSeconds(0.01f);
    }

}
