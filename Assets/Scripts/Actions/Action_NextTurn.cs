using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/*This script is separated into two parts:
    Pre_NextTurn - This handles all units attacking and scoring objectives
    NextTurn - This handles actual turn and round change
*/
public static class Action_NextTurn
{
    static bool IsChangingTurn;
    static bool IsChangingToNextTurn;

    static bool ShouldDebug = false;

    public static IEnumerator NextTurn(ActionParameters parameters)
    {
        yield return NextTurn();

        yield return new WaitForSeconds(Time.fixedDeltaTime);
        GameManager.Instance.RemoveAction(parameters);
    }

    public static IEnumerator NextTurn()
    {
        yield return BattleStatsManager.Instance.NextTurn();
    }
}
