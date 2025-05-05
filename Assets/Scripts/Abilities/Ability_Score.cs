using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.Windows;
using UnityEngine.Events;

public static class Ability_Score
{
    const float DelayBetweenScores = 40;
    public static UnityEvent E_ScoredForPlayer= new UnityEvent();

    const string JitterAnimTrigger = "jitter";

    static bool ShouldDebug = false;

    public static int Check(Unit obj)
    {
        var l = BoardManager.Instance.Get_UnitsInRange(obj, 1);
        if (ShouldDebug) foreach(var v in l) { Debug.Log("all: " + v.gameObject.name); }
        if (l == null || l.Count <= 0) return 0;

        if (ShouldDebug) Debug.Log("---");
        List<Unit> pU = GameManager.Instance.Get_OnlyUnitsWithKeywords(l, new List<Keyword> { Keyword.Player });
        if (ShouldDebug) foreach (var v in pU) { Debug.Log("player - " + v.gameObject.name); }

        List<Unit> eU = GameManager.Instance.Get_OnlyUnitsWithKeywords(l, new List<Keyword> { Keyword.Enemy });
        if (ShouldDebug) foreach (var v in eU) { Debug.Log("enemy - " + v.gameObject.name); }

        return pU.Count - eU.Count;

    }

    //Main function, goes through every objective on the map and scores appropriately (checks for the keywords of surrounding units)
    public static IEnumerator GlobalScore(ActionParameters parameters)
    {
        List<Keyword> keywords = parameters.Keywords;
        yield return new WaitForSeconds(Time.deltaTime);

        List<Keyword> objs = new List<Keyword> { Keyword.Objective };
        List<Unit> all = BoardManager.Instance.Get_AllUnitsWithKeywords(objs);

        foreach (var a in all)
        {
            int res = Check(a);
            if (ShouldDebug) Debug.Log("result for " + a.gameObject.name + " is " + Check(a));

            #region Player scores
            if (res > 0 && ScoreManager.Instance.CurTurn == Side.Player)
            {
                a.Anim.SetTrigger(JitterAnimTrigger);
                ScoreClock.Instance.Shrug_Visuals();
                E_ScoredForPlayer.Invoke();
                ScoreManager.Instance.AddPointPlayer();
            }
            #endregion
            #region Enemy scores
            if (res < 0 && ScoreManager.Instance.CurTurn == Side.Enemy)
            {
                a.Anim.SetTrigger(JitterAnimTrigger);
                ScoreClock.Instance.Shrug_Visuals();

                ScoreManager.Instance.AddPointEnemy();
            }
            #endregion
            #region Neither scores
            //TODO:
            #endregion

            yield return new WaitForSeconds(DelayBetweenScores * Time.deltaTime);
        }

        yield return new WaitForSeconds(2 * Time.deltaTime);
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
