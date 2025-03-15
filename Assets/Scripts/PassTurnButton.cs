using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassTurnButton : MonoBehaviour
{
    public void Pass()
    {
        if (ScoreManager.Instance.CurTurn == ScoreManager.Side.Player)
        {
            ActionParameters parameters = new ActionParameters(GameManager.ActionType.NextTurn, null, null, null, null, 0);
            StartCoroutine(GameManager.Instance.Action(parameters));
        }
    }
}
