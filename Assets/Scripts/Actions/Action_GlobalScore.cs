using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class Action_GlobalScore
{
    static float DelayBetweenScores = 20;
    public static UnityEvent E_ScoredForPlayer = new UnityEvent();
    static bool ShouldDebug = false;

    public static IEnumerator GlobalScore(ActionParameters parameters)
    {
        yield return new WaitForSeconds(Time.fixedDeltaTime / 100);
        if (ShouldDebug) Debug.Log("GlobalScore");

        List<Keyword> keywords = parameters.Keywords;
        yield return new WaitForSeconds(Time.deltaTime);

        List<Keyword> objs = new List<Keyword> { Keyword.Objective };
        List<Unit> all = BoardManager.Instance.Get_AllUnitsWithKeywords(objs);

        foreach (var unit in all)
        {
            if (unit.Get_AbilityInstance(Ability_Name.Score).Ability_ is Ability_Score) { } else { Debug.LogError("ABILITY IS NOT SCORE"); }

            Ability_Score ability_score = unit.Get_AbilityInstance(Ability_Name.Score).Ability_ as Ability_Score;
            if (ability_score != null)
            {

                int res = ability_score.Check();
                if (ShouldDebug) Debug.Log("result for " + unit.gameObject.name + " is " + ability_score.Check());

                #region Player scores
                if (res > 0 && BattleStatsManager.Instance.CurTurn == Side.Player)
                {
                    unit.Anim.SetTrigger(ability_score.JitterAnimTrigger);
                    ScoreClock.Instance.Shrug_Visuals();
                    E_ScoredForPlayer.Invoke();
                    BattleStatsManager.Instance.AddPointPlayer();
                }
                #endregion
                #region Enemy scores
                else if (res < 0 && BattleStatsManager.Instance.CurTurn == Side.Enemy)
                {
                    unit.Anim.SetTrigger(ability_score.JitterAnimTrigger);
                    ScoreClock.Instance.Shrug_Visuals();

                    BattleStatsManager.Instance.AddPointEnemy();
                }
                #endregion
                #region Neither scores
                else
                {
                    if (ShouldDebug) Debug.Log("Neither side scores");
                }
                #endregion
            }
            else
            {
                if (ShouldDebug) Debug.Log("Ability is null for " + unit.UnitName + " " + ability_score.aName);
            }

            yield return new WaitForSeconds(DelayBetweenScores * Time.fixedDeltaTime);
        }

        yield return new WaitForSeconds(Time.fixedDeltaTime / 100);
        GameManager.Instance.RemoveAction(parameters);
    }
}
