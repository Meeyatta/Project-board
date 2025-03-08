using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Continuously checks if conditions for specific unit ablities are satisfied, if so, triggers them

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance;
    Coroutine CurAbilities = null;
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
    }
    void Awake()
    {
        Instance = this;
    }

    IEnumerator CheckForAbilities()
    {
        yield return new WaitForSeconds(0.01f); 

        List<Unit> all = BoardManager.Instance.Get_AllUnitsOnBoard();
        if (all != null && all.Count > 0)
        {
            foreach (var v in all)
            {
                //Score is handled as a single call
            }
        }

        CurAbilities = null;
        yield return new WaitForSeconds(0.01f);
    }

    private void FixedUpdate()
    {
        if (CurAbilities == null)
        {
            CurAbilities = StartCoroutine(CheckForAbilities());
        }
        else
        {
            //Debug.Log("CurAbilities is not null");
        }
    }

}
