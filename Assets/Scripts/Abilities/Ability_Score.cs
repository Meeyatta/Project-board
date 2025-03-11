using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.Windows;

public static class Ability_Score 
{
    //Checks individual objective unit if it can score for player/enemy
    public static bool Check(Unit u, List<Keyword> keywords)
    {
        if (!u.CurAbilities.Contains(Ability.Score)) return false;

        //TODO: Add a check if it IS an objective, but doesn't need scoring
        var l = BoardManager.Instance.Get_UnitsInRange(u, 1);
        if (l == null || l.Count <= 0) return false;


        List<Unit> totl = new List<Unit>();
        foreach ( var k in keywords )
        {
            List<Keyword> keyw = new List<Keyword> { k };
            totl.AddRange(GameManager.Instance.Get_OnlyUnitsWithKeywords(l, keyw));
        }

        if (totl == null || totl.Count <= 0) return false;

        return true;

    }
    //Main function, goes through every objective on the map and scores appropriately (checks for the keywords of surrounding units)
    public static IEnumerator GlobalScore(ActionParameters parameters)
    {
        List<Keyword> keywords = parameters.Keywords;
        yield return new WaitForSeconds(0.001f);

        List<Keyword> ks = new List<Keyword> { Keyword.Objective };
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

        yield return new WaitForSeconds(0.001f);
        GameManager.Instance.RemoveAction(parameters);
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

    //Scoring for an individual objective unit
    public static IEnumerator Score(List<Unit> ActionTargetUnits, List<Keyword> keywords)
    {
        yield return new WaitForSeconds(0.001f);

        foreach (var u in ActionTargetUnits)
        {
            var l = BoardManager.Instance.Get_UnitsInRange(u, 1);

            List<Keyword> pk = new List<Keyword> { Keyword.Player };
            List<Unit> pu = GameManager.Instance.Get_OnlyUnitsWithKeywords(l, pk);

            List<Keyword> ek = new List<Keyword> { Keyword.Enemy };
            List<Unit> eu = GameManager.Instance.Get_OnlyUnitsWithKeywords(l, ek);

            if (PlayerScoreConditions(pu, eu, keywords)) { ScoreManager.Instance.Score_Player++; }

            if (EnemyScoreConditions(pu, eu, keywords)) { ScoreManager.Instance.Score_Enemy++; }

        }
        yield return new WaitForSeconds(0.01f);
    }

}
