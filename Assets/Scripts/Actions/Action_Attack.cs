using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Attack : MonoBehaviour
{
    const string AttackAnimTrigger = "attack";
    public static Action_Attack Instance;
    void Singleton()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(this);
    }
    private void Awake()
    {
        Singleton();
    }

    public IEnumerator Attack(List<Unit> ActionTargetUnits)
    {
        Debug.Log("Actually reached sorting it");

        foreach (var unit in OrderedUnits(ActionTargetUnits))
        {
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
            if (unit.Keywords.Contains(Unit.Keyword.Player)) { keywords.Add(Unit.Keyword.Enemy); }
            else { keywords.Add(Unit.Keyword.Player); }

            yield return StartCoroutine(DamageAllInRange(unit, keywords));
        }


        yield return new WaitForSeconds(0.001f);
    }

    List<Unit> OrderedUnits(List<Unit> ActionTargetUnit)
    {
        Debug.Log("We got to sorting units");
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
    IEnumerator DamageAllInRange(Unit source, List<Unit.Keyword> keywords)
    {
        List<Unit> targets = GameManager.Instance.GetPossibleTargets(source, keywords);


        foreach (Unit target in targets) 
        {
            yield return StartCoroutine(Damage(target, source.CurAttackZone.Damage, source));
        }

        yield return new WaitForSeconds(1);
    }
    IEnumerator Damage(Unit target, int damage, Unit source)
    {
        target.CurrentHealth = Mathf.Clamp(target.CurrentHealth - damage, 0, target.CurrentHealth);

        if (target.CurrentHealth <= 0) { yield return StartCoroutine(Kill(target, source)); }

        yield return new WaitForSeconds(0.1f);
    }
    IEnumerator Kill(Unit target, Unit source)
    {
        Debug.Log(target.UnitName + " has been killed by " + source.UnitName);
        yield return new WaitForSeconds(0.1f);
        target.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.1f);
    }
    
}
