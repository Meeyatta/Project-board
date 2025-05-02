using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class Action_SelectPosition 
{
    public static Unit CurUnit;
    public static UnityEvent<List<Vector2Int>> ESendPositionBack;
    public static bool IsAwaitingAClickBack = true;
    static bool ShouldCancel = false;

    static void Cancel()
    {
        Debug.Log("SelectPositionCancel");
        ShouldCancel = true;
    }

    public static IEnumerator Selecting(Unit unit, bool canCancel)
    {
        if (canCancel) GameManager.Instance.CancelEvent.AddListener(Cancel);

        void StopWaiting(Vector2Int v)
        {
            //Debug.Log("Stopped waiting");
            IsAwaitingAClickBack = false;

            if (CurUnit != null)
            {
                for (int i = 0; i < CurUnit.Size.Positions.Count; i++)
                {
                    List<Vector2Int> nv = new List<Vector2Int>();

                    for (int ii = 0; ii < unit.Size.Positions.Count; ii++)
                    {
                        nv.Add(v + unit.Size.Positions[i] - unit.Size.Positions[ii]);
                    }
                    //Debug.Log("Will send " + (v[i] + CurUnit.Size.Positions[i]) + " back to GameManager");

                    bool areAll = true;
                    foreach (var pos in nv)
                    {
                        if (!BoardManager.Instance.IsInBounds(pos)) { areAll = false; }
                    }

                    //If all of these coordinates are within a border, return  these positions
                    if (areAll)
                    {
                        ESendPositionBack.Invoke(nv);
                    }

                }

            }

        }
        GameManager.Instance.ClickBackEvent.AddListener(StopWaiting);
        //Debug.Log("Added a listener to clickback");

        CurUnit = unit;

        IsAwaitingAClickBack = true;
        
        while (IsAwaitingAClickBack && !canCancel || (IsAwaitingAClickBack && !ShouldCancel && canCancel)) 
        { yield return new WaitForSeconds(Time.deltaTime); }

        if (ShouldCancel && canCancel)
        {
            Debug.Log("SELECTPOSITION ACTION IS CANCELED");
            ShouldCancel = false;
            List<Unit> nv = new List<Unit>(); nv.Add(unit);
            GameManager.Instance.E_HidePlacement.Invoke(nv);
            if (canCancel) GameManager.Instance.CancelEvent.RemoveListener(Cancel);
            yield break;
        }

        GameManager.Instance.ClickBackEvent.RemoveListener(StopWaiting);
        ShouldCancel = false;
        if (canCancel) GameManager.Instance.CancelEvent.RemoveListener(Cancel);
        yield return new WaitForSeconds(Time.fixedDeltaTime);
    }

}
