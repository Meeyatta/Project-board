using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class Action_SelectUnitPosition 
{
    public static Unit CurUnit;
    public static UnityEvent<List<Vector2Int>> ESendPositionBack = new UnityEvent<List<Vector2Int>>();
    public static bool IsAwaitingAClickBack = true;
    static bool ShouldCancel = false;

    static bool ShouldDebug = true;

    static void Cancel()
    {
        Debug.Log("SelectPositionCancel");
        ShouldCancel = true;
    }

    //This selects a position for items, thus ignores positions it is not going to fit into
    public static IEnumerator Selecting_relaxed(List<Vector2Int> poss, bool canCancel)
    {
        if (canCancel) GameManager.Instance.CancelEvent.AddListener(Cancel);

        void StopWaiting(Vector2Int v)
        {
            if (ShouldDebug) Debug.Log("Stopped waiting - selecting_relaxed sent coordinates of " + v);
            IsAwaitingAClickBack = false;

            ESendPositionBack.Invoke(new List<Vector2Int> { v });

        }

        if (ShouldDebug) Debug.Log("Added a listener to clickback");
        ClickManager.Instance.ClickBackEvent.AddListener(StopWaiting);

        IsAwaitingAClickBack = true;

        while (IsAwaitingAClickBack && !canCancel || (IsAwaitingAClickBack && !ShouldCancel && canCancel))
        { yield return new WaitForSeconds(Time.deltaTime); }

        if (ShouldCancel && canCancel)
        {
            Debug.Log("SELECTPOSITION_RELAXED ACTION IS CANCELED");
            ShouldCancel = false;

            /* TODO: ADD THE EFFECT HIDING FOR THE COVER EFFECTS
             * 
            List<Unit> nv = new List<Unit>(); nv.Add(unit);
            GameManager.Instance.E_HidePlacement.Invoke(nv);
            */

            if (canCancel) GameManager.Instance.CancelEvent.RemoveListener(Cancel);
            yield break;
        }

        ClickManager.Instance.ClickBackEvent.RemoveListener(StopWaiting);
        ShouldCancel = false;
        if (canCancel) GameManager.Instance.CancelEvent.RemoveListener(Cancel);
        yield return new WaitForSeconds(Time.fixedDeltaTime);
    }

    //THIS selects a position for a unit, thus also considers if unit can fit into said position
    public static IEnumerator Selecting_strict(Unit unit, bool canCancel)
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

                    
                    bool areAll = BoardManager.Instance.AreInBounds(nv);

                    //If all of these coordinates are within a border, return  these positions
                    if (areAll)
                    {
                        ESendPositionBack.Invoke(nv);
                    }

                }

            }

        }
        ClickManager.Instance.ClickBackEvent.AddListener(StopWaiting);
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

        ClickManager.Instance.ClickBackEvent.RemoveListener(StopWaiting);
        ShouldCancel = false;
        if (canCancel) GameManager.Instance.CancelEvent.RemoveListener(Cancel);
        yield return new WaitForSeconds(Time.fixedDeltaTime);
    }

}
