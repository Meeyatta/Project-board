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
    public bool ShouldDebug;

    IEnumerator StartBattle()
    {
        yield return new WaitForSeconds(Delay * Time.fixedDeltaTime);

        #region Creating the battlefield map
        ActionParameters pars = new ActionParameters(GameManager.ActionType.Create, null, null, Objective_Positions, null, 0);
        yield return Action_CreateBattlefield.CreateBattlefield(pars);
        #endregion

        #region Placing enemy units
        yield return EnemyManager.Instance.InstantiateEnemyUnits();
        EnemyManager.Instance.ResetArmy();
        yield return EnemyManager.Instance.DeployEnemies();
        #endregion

        #region Placing player units
        yield return PlayerManager.Instance.InstantiatePlayerUnits();
        PlayerManager.Instance.ResetArmy();
        yield return PlayerManager.Instance.DeployPlayerUnits();
        #endregion

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
