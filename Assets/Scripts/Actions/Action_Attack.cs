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
                foreach (Vector2Int unitP in BoardManager.Instance.Get_UnitPositions(source))
                {
                    if (!BoardManager.Instance.IsInBounds(line.Positions[i] + unitP)) { break; }
                    int x = line.Positions[i].x; int y = line.Positions[i].y;
                    if (BoardManager.Instance.Board[unitP.x + x].Cells[unitP.y + y].CurUnit != null && !line.IsEvading)
                    {
                        Unit curUn = BoardManager.Instance.Board[unitP.x + x].Cells[unitP.y + y].CurUnit;
                        if (!res.Contains(curUn) && GameManager.Instance.UnitHasAllKeywords(curUn, keywords)) { res.Add(curUn); }
                        break;
                    }

                }

            }
        }
        #endregion
        return res;

    }
    
    //Makes units in a list initiate an attack
    public static IEnumerator Attack(ActionParameters parameters)
    {
        List<Unit> ActionTargetUnits = parameters.ActionTargetUnits;

        foreach (var unit in DamageDealing.OrderedUnits(ActionTargetUnits))
        {
            if (!IsAbleToAttack(unit)) { continue; }

            Animator anim = unit.Anim;
            anim.SetTrigger(AttackAnimTrigger);

            while (!anim.GetBool("IsAttacking"))
            {
                yield return new WaitForSeconds(0.01f);
            }

            while (anim.GetBool("IsAttacking"))
            {
                yield return new WaitForSeconds(0.01f);
            }
            //At the end of animation, damage all of the units
            List<Keyword> keywords = new List<Keyword>();
            if (unit.CurKeywords.Contains(Keyword.Player)) { keywords.Add(Keyword.Enemy); }
            else { keywords.Add(Keyword.Player); }

            yield return GameManager.Instance.StartCoroutine(DamageAllInRange(unit, keywords));
        }


        yield return new WaitForSeconds(0.001f);
        Debug.Log("Sent what ended the attack");
        GameManager.Instance.RemoveAction(parameters);
    }
    public static IEnumerator DamageAllInRange(Unit source, List<Keyword> keywords)
    {
        List<Unit> targets = GetPossibleTargets(source, keywords);


        foreach (Unit target in targets) 
        {
            yield return GameManager.Instance.StartCoroutine(
                DamageDealing.Damage(target, source));
        }

        yield return new WaitForSeconds(0.01f);
    }

    
}
