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

    public static IEnumerator Damage(Unit target, Unit source)
    {

        int startDamage = source.CurAttackZone.Damage; int EndDamage = startDamage;
        DamageType type = source.CurAttackZone.Type;

        #region Modifying damage depending on abilities
        switch (type)
        {
            case DamageType.Fire:
                if (target.CurAbilities.Contains(Ability.Fireproof)) { EndDamage -= 1; }
                if (target.CurAbilities.Contains(Ability.Flammable)) { EndDamage += 1; }
                break;
            case DamageType.Lightning:
                if (target.CurAbilities.Contains(Ability.Nonconductive)) { EndDamage -= 1; }
                if (target.CurAbilities.Contains(Ability.Conductive)) { EndDamage += 1; }
                break;
            case DamageType.Physical:
                if (target.CurAbilities.Contains(Ability.Armored)) { EndDamage -= 1; }
                if (target.CurAbilities.Contains(Ability.Exposed)) { EndDamage += 1; }
                break;
        }
        #endregion
        if (target.CurAbilities.Contains(Ability.Invincible) || EndDamage < 0) { EndDamage = 0; }

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
        Debug.Log(target.UnitName + " has been killed by " + source.UnitName);

        #region Graphical stuff
        target.Anim.SetTrigger(DieAnimTrigger);
        #endregion

        yield return new WaitForSeconds(Time.deltaTime * 35);
        target.gameObject.SetActive(false);

        yield return new WaitForSeconds(Time.deltaTime);
    }
}
