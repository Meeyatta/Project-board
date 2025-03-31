using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.Windows;

public static class Ability_Score
{
    const string JitterAnimTrigger = "jitter";
    public static int Check(Unit obj)
    {
        var l = BoardManager.Instance.Get_UnitsInRange(obj, 1);
        if (l == null || l.Count <= 0) return 0;

        List<Unit> pU = GameManager.Instance.Get_OnlyUnitsWithKeywords(l, new List<Keyword> { Keyword.Player });
        List<Unit> eU = GameManager.Instance.Get_OnlyUnitsWithKeywords(l, new List<Keyword> { Keyword.Enemy });

        return pU.Count - eU.Count;

    }

    //Main function, goes through every objective on the map and scores appropriately (checks for the keywords of surrounding units)
    public static IEnumerator GlobalScore(ActionParameters parameters)
    {
        List<Keyword> keywords = parameters.Keywords;
        yield return new WaitForSeconds(Time.deltaTime);

        List<Keyword> ks = new List<Keyword> { Keyword.Objective };
        List<Unit> all = BoardManager.Instance.Get_AllUnitsWithKeywords(ks);

        foreach (var a in all)
        {
            int res = Check(a);

            #region Player scores
            if (res > 0)
            {
                a.Anim.SetTrigger(JitterAnimTrigger);
                ScoreManager.Instance.AddPointPlayer();
            }
            #endregion
            #region Enemy scores
            if (res < 0)
            {
                a.Anim.SetTrigger(JitterAnimTrigger);
                ScoreManager.Instance.AddPointEnemy();
            }
            #endregion
            #region Neither scores
            //TODO:
            #endregion

        }

        yield return new WaitForSeconds(Time.deltaTime);
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


}
