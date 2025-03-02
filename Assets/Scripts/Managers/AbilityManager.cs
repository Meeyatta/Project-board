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

        if (Ability_Score.Check() != null && Ability_Score.Check().Count >= 1) 
        {
            ActionParameters parameters = new ActionParameters(GameManager.ActionType.Score, Ability_Score.Check(), null, null, null);
            yield return StartCoroutine(GameManager.Instance.Action(parameters));
        }

        CurAbilities = null;
        yield return new WaitForSeconds(0.01f);
    }

    private void FixedUpdate()
    {
        if (CurAbilities != null)
        {
            CurAbilities = StartCoroutine(CheckForAbilities());
        }
    }

}
