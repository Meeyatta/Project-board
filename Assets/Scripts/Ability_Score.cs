using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Ability_Score 
{
    public static List<Unit> Check()
    {
        List<Unit> Checked = new List<Unit>();
        List<Unit.Keyword> l = new List<Unit.Keyword>(); l.Add(Unit.Keyword.Objective);
        Checked = BoardManager.Instance.Get_AllUnitsWithKeywords(l);

        //TODO: Add a check what removes all objectives what don't need scoring

        return Checked;

    }
    public static IEnumerator Score(List<Unit> ActionTargetUnits)
    {
        //TODO: Add functionality to add points appropriately
        yield return new WaitForSeconds(0.01f);
    }

}
