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

    //TODO: Sorting doesn't work as it should, the order of attacking units is wrong
    List<Unit> OrderedUnits(List<Unit> ActionTargetUnit)
    {
        Debug.Log("We got to sorting units");
        //TODO: Fix this absolutely terrible sorting algorythm to something better

        List<Unit> temp = ActionTargetUnit;

        //The sorting algoryth doesn't work for a single element, so this needs to be done
        if (temp.Count <= 1) { return temp; }

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
                Vector2Int lastPositionip = BoardManager.Instance.Get_UnitPositions(temp[i+1])[positions.Count - 1];

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

        //if (justincase == 0) { Debug.Log("THE LOOP IS INFINITE"); } else { Debug.Log(justincase); }

        return temp;


    }
    public IEnumerator Attack(List<Unit> ActionTargetUnits)
    {
        Debug.Log("Actually reached sorting it");

        foreach (var v in OrderedUnits(ActionTargetUnits))
        {
            Animator anim = v.Anim;
            anim.SetTrigger(AttackAnimTrigger);

            while (!anim.GetBool("IsAttacking"))
            {
                Debug.Log("Waiting for the bool");
                yield return new WaitForSeconds(0.01f);
            }

            while (anim.GetBool("IsAttacking"))
            {
                Debug.Log("Bool is true");
                yield return new WaitForSeconds(0.01f);
            }
            //At the end of animation, damage all of the units

        }


        yield return new WaitForSeconds(0.001f);
    }
}
