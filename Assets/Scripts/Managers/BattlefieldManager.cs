using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BattlefieldManager : MonoBehaviour
{
    #region Singleton
    public static BattlefieldManager Instance;
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
    void Awake()
    {
        Singleton();
    }
    #endregion

    public List<Vector2Int> Objective_Positions;
    public GameObject Objective_Obj;

    [Header("---Functionality stuff---")]
    public float Delay;
    public UnityEvent eBattlefieldCreation;


    IEnumerator MakeBattlefield(List<Vector2Int> objective_Positions)
    {
        #region Place objectives
        foreach (var p in objective_Positions)
        {
            List<Vector2Int> pos = new List<Vector2Int> { p };
            ActionParameters paramets = new ActionParameters(
                        GameManager.ActionType.Create, null, null, pos, Objective_Obj, 0);
            yield return StartCoroutine( GameManager.Instance.Action(paramets));
        }

        #endregion
    }
    IEnumerator StartBattle()
    {
        yield return new WaitForSeconds(Delay * Time.fixedDeltaTime);
        yield return MakeBattlefield(Objective_Positions);

        yield return EnemyManager.Instance.InstantiateEnemyUnits();
        EnemyManager.Instance.ResetArmy();
        yield return EnemyManager.Instance.DeployEnemies();

        yield return Action_DrawNewItem.DrawNewItem();

        yield return PlayerManager.Instance.InstantiatePlayerUnits();
        PlayerManager.Instance.ResetArmy();
        yield return PlayerManager.Instance.DeployPlayerUnits();

        

        eBattlefieldCreation.Invoke();

    }
    void Start()
    {
        StartCoroutine(StartBattle());
    }

    void Update()
    {
        
    }
}
