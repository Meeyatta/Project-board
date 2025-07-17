using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Allows for a group of units to attack or returns what possible units can be attacked by the unit
public static class Action_Attack 
{
    const string AttackAnimTrigger = "attack";

    //Check if the unit isn't meant to/can't attack
    public static bool IsAbleToAttack(Unit u)
    {
        if (!BoardManager.Instance.IsOnBoard(u) || u.CurKeywords.Contains(Keyword.Neutral)) { return false; }

        return true;
    }
    
    //Returns all possible units what a source (unit) can affect using their current attack Zones
    public static List<Unit> GetPossibleTargets(Unit source, List<Keyword> keywords)
    {
        List<Unit> res = new List<Unit>();

        #region This goes through each individaul line and stops if it encounters a unit
        if (source.CurAttackZone.Lines.Count == 0) { return res; }

        foreach (var line in source.CurAttackZone.Lines)
        {
            for (int i = 0; i < line.Positions.Count; i++)
            {
                bool canGoFurther = true;

                foreach (Vector2Int unitP in BoardManager.Instance.Get_UnitPositions(source))
                {
                    if (!BoardManager.Instance.IsInBounds(line.Positions[i] + unitP)) { canGoFurther = false; break; }
                    int x = line.Positions[i].x; int y = line.Positions[i].y;
                    if (BoardManager.Instance.Board[unitP.x + x].Cells[unitP.y + y].CurUnit != null && !line.IsEvading)
                    {
                        canGoFurther = false;
                        Unit curUn = BoardManager.Instance.Board[unitP.x + x].Cells[unitP.y + y].CurUnit;
                        if (!res.Contains(curUn) && GameManager.Instance.UnitHasAllKeywords(curUn, keywords)) { res.Add(curUn); }
                        break;
                    }

                }

                if (!canGoFurther) break;
            }
        }
        #endregion
        return res;

    }
    
    public static IEnumerator UnitAttack(Unit unit)
    {
        if (!IsAbleToAttack(unit)) { yield break; }

        Animator anim = unit.Anim;
        anim.SetTrigger(AttackAnimTrigger);

        float MaxTime = 10;
        while (!anim.GetBool("isAttacking") && MaxTime > 0)
        {
            MaxTime -= Time.fixedDeltaTime;
            yield return new WaitForSeconds(Time.fixedDeltaTime / 10);
        }

        MaxTime = 10;
        while (anim.GetBool("isAttacking") && MaxTime > 0)
        {
            MaxTime -= Time.fixedDeltaTime;
            yield return new WaitForSeconds(Time.fixedDeltaTime / 10);
        }

        yield return new WaitForSeconds(Time.fixedDeltaTime);

        List<Keyword> keywords = new List<Keyword>();
        if (unit.CurKeywords.Contains(Keyword.Player)) { keywords.Add(Keyword.Enemy); }
        else { keywords.Add(Keyword.Player); }

        yield return DamageAllInRange(unit, keywords);
    }

    //Makes units in a list initiate an attack
    public static IEnumerator Attack(ActionParameters parameters)
    {
        List<Unit> ActionTargetUnits = parameters.ActionTargetUnits;
        if (ActionTargetUnits == null || ActionTargetUnits.Count == 0) { yield break; }

        foreach (var unit in DamageDealing.OrderedUnits(ActionTargetUnits))
        {
            yield return UnitAttack(unit);
        }

        yield return new WaitForSeconds(Time.deltaTime);
        GameManager.Instance.RemoveAction(parameters);
    }
    public static IEnumerator DamageAllInRange(Unit source, List<Keyword> keywords)
    {
        if (source == null || keywords == null || keywords.Count == 0) { yield break; }
        List<Unit> targets = GetPossibleTargets(source, keywords);

        foreach (Unit target in targets) 
        {
            yield return GameManager.Instance.StartCoroutine(DamageDealing.Damage(target, source));
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }

        yield return new WaitForSeconds(Time.fixedDeltaTime);
    }

    
}
