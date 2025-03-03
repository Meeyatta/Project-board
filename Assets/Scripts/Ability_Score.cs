using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Ability_Score 
{
    public static bool Check(Unit u)
    {
        if (!u.Keywords.Contains(Unit.Keyword.Objective)) return false;

        //TODO: Add a check if it IS an objective, but doesn't need scoring
        var l = BoardManager.Instance.Get_UnitsInRange(u, 1);
        if (l == null || l.Count <= 0) return false;    

        return true;

    }
    public static IEnumerator Score(List<Unit> ActionTargetUnits)
    {
        yield return new WaitForSeconds(0.01f);

        foreach (var u in ActionTargetUnits)
        {
            //TODO: Add functionality to add points appropriately

            var l = BoardManager.Instance.Get_UnitsInRange(u, 1);

            List<Unit.Keyword> pk = new List<Unit.Keyword> { Unit.Keyword.Player };
            List<Unit> pu = GameManager.Instance.Get_OnlyUnitsWithKeywords(l, pk);

            List<Unit.Keyword> ek = new List<Unit.Keyword> { Unit.Keyword.Enemy };
            List<Unit> eu = GameManager.Instance.Get_OnlyUnitsWithKeywords(l, ek);

            if (pu != null && pu.Count > 0 && (eu == null || eu.Count <= 0)) { ScoreManager.Instance.Score_Player++; }

            if (eu != null && eu.Count > 0 && (pu == null || pu.Count <= 0)) { ScoreManager.Instance.Score_Enemy++; }

        }
        yield return new WaitForSeconds(0.01f);
    }

}
