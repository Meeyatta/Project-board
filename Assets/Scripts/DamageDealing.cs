using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum DamageType
{
    Physical,
    Fire,
    Lightning,
    Poison, //Untested
    Frost,  //Untested

}
public static class DamageDealing
{
    const string ShrugAnimTrigger = "shrug";
    const string DieAnimTrigger = "die";
    public static List<Unit> OrderedUnits(List<Unit> ActionTargetUnits)
    {

        //TODO: Fix this absolutely terrible sorting algorythm to something better

        List<Unit> temp = ActionTargetUnits;

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
         
                List<Vector2Int> l1 = BoardManager.Instance.Get_UnitPositions(temp[i]); List<Vector2Int> l2 = BoardManager.Instance.Get_UnitPositions(temp[i + 1]);
                if (positions.Count - 1 < 0 || positions.Count - 1 >= l1.Count || positions.Count - 1 >= l2.Count) return temp;

                Vector2Int lastPositioni = l1[positions.Count - 1];
                Vector2Int lastPositionip = l2[positions.Count - 1];

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

    public static IEnumerator Damage(Unit target, Unit source)
    {

        int startDamage = source.CurAttackZone.Damage; 
        DamageType type = source.CurAttackZone.Type;

        int EndDamage = AbilityManager.Instance.ModifyIncomingDamage(target, startDamage, type); //Modifying damage depending on abilities

        if (target.HasAbility(Ability_Name.Invincible) || EndDamage < 0) 
        { 
            EndDamage = 0;
            yield return new WaitForSeconds(Time.deltaTime);
            yield break; 
        }

        //Debug.Log(source.gameObject.name + " has dealt " + EndDamage + " " + type + " damage to " + target.gameObject.name);

        #region Graphical stuff
        target.Anim.SetTrigger(ShrugAnimTrigger);
        #endregion

        #region Dealing damage and checking if the enemy died
        target.CurrentHealth = Mathf.Clamp(target.CurrentHealth - EndDamage, 0, target.CurrentHealth);
        AudioManager.Instance.Play(SoundName.Damaged, target.transform);
        if (target.CurrentHealth <= 0) { yield return GameManager.Instance.StartCoroutine(Kill(target, source)); }
        #endregion

        yield return new WaitForSeconds(Time.deltaTime);
    }
    public static IEnumerator Kill(Unit target, Unit source)
    {
       // Debug.Log(target.UnitName + " has been killed by " + source.UnitName);

        #region Graphical stuff
        target.Anim.SetTrigger(DieAnimTrigger);
        #endregion

        yield return new WaitForSeconds(Time.fixedDeltaTime);

        target.gameObject.SetActive(false);
        foreach(var v in BoardManager.Instance.Get_UnitPositions(target))
        {
            BoardManager.Instance.Board[v.x].Cells[v.y].CurUnit = null;
        }

        yield return new WaitForSeconds(Time.fixedDeltaTime);
    }
}
