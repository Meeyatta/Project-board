using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Action_AttackFromKeyworded 
{
    const string AttackAnimTrigger = "attack";

    //Check if the unit isn't meant to/can't attack
    public static bool IsAbleToAttack(Unit u)
    {
        if (!BoardManager.Instance.IsOnBoard(u) || u.CurKeywords.Contains(Unit.Keyword.Neutral)) { return false; }

        return true;
    }

    //Returns all possible units what a source (unit) can affect using their current attack Zones
    public static List<Unit> GetPossibleTargets(Unit source, List<Unit.Keyword> keywords)
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
                        if (!res.Contains(curUn) && GameManager.Instance.UnitHasAllKeywords(curUn, keywords)) { Debug.Log("Added " + curUn.UnitName);  res.Add(curUn); }
                        break;
                    }

                }

            }
        }
        #endregion
        return res;

    }

    //Makes units in a list initiate an attack
    public static IEnumerator AttackFromKeyworded(ActionParameters parameters)
    {
        List<Unit.Keyword> Keywords = parameters.Keywords;

        List<Unit> affected = BoardManager.Instance.Get_AllUnitsWithKeywords(Keywords);



        foreach (var unit in OrderedUnits(affected))
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
            List<Unit.Keyword> keywords = new List<Unit.Keyword>();
            if (unit.CurKeywords.Contains(Unit.Keyword.Player)) { keywords.Add(Unit.Keyword.Enemy); }
            else { keywords.Add(Unit.Keyword.Player); }

            yield return GameManager.Instance.StartCoroutine(DamageAllInRange(unit, keywords));
        }


        yield return new WaitForSeconds(0.001f);
        Debug.Log("Sent what ended the attack");
        GameManager.Instance.RemoveAction(parameters);
    }

    public static List<Unit> OrderedUnits(List<Unit> ActionTargetUnit)
    {
        //TODO: Fix this absolutely terrible sorting algorythm to something better

        List<Unit> temp = ActionTargetUnit;

        //The sorting algoryth doesn't work for a single element, so this needs to be done
        if (temp.Count <= 1) { return temp; }

        #region Sorting algorythm
        long justincase = 999999;
        bool needsSorting = true;
        while (needsSorting && justincase > 0)
        {
            justincase--;
            for (int i = 0; i < temp.Count - 1; i++)
            {
                needsSorting = false;

                List<Vector2Int> positions = BoardManager.Instance.Get_UnitPositions(temp[i]);
                Vector2Int lastPositioni = BoardManager.Instance.Get_UnitPositions(temp[i])[positions.Count - 1];
                Vector2Int lastPositionip = BoardManager.Instance.Get_UnitPositions(temp[i + 1])[positions.Count - 1];

                int xi = lastPositioni.x;
                int yi = lastPositioni.y;

                int xip = lastPositionip.x;
                int yip = lastPositionip.y;

                //If the next unit is higher than our previous unit
                if (yip < yi)
                {
                    //Debug.Log();

                    needsSorting = true;
                    Unit ip = temp[i + 1];
                    temp[i + 1] = temp[i]; temp[i] = ip;
                }
                //If the next unit has the same height as our previous unit
                else if (yip == yi)
                {
                    //If the next unit is left to our previous unit
                    if (xip < xi)
                    {
                        needsSorting = true;
                        Unit ip = temp[i + 1];
                        temp[i + 1] = temp[i]; temp[i] = ip;
                    }
                }

            }
        }

        #endregion

        return temp;

    }
    public static IEnumerator DamageAllInRange(Unit source, List<Unit.Keyword> keywords)
    {
        List<Unit> targets = GetPossibleTargets(source, keywords);

        foreach (Unit target in targets)
        {
            yield return GameManager.Instance.StartCoroutine(Damage(target, source.CurAttackZone.Damage, source));
        }

        yield return new WaitForSeconds(1);
    }
    public static IEnumerator Damage(Unit target, int damage, Unit source)
    {
        target.CurrentHealth = Mathf.Clamp(target.CurrentHealth - damage, 0, target.CurrentHealth);

        if (target.CurrentHealth <= 0) { yield return GameManager.Instance.StartCoroutine(Kill(target, source)); }

        yield return new WaitForSeconds(0.1f);
    }
    public static IEnumerator Kill(Unit target, Unit source)
    {
        Debug.Log(target.UnitName + " has been killed by " + source.UnitName);
        yield return new WaitForSeconds(0.1f);
        target.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.1f);
    }
}
