using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class BattleParameters
{
    public int BoardWidth; public int BoardHeight;
    public List<Vector2Int> ObjectiveCoords;
    public Vector2Int Player_deployment_start; public Vector2Int Player_deployment_end;
    public Vector2Int Enemy_deployment_start; public Vector2Int Enemy_deployment_end;
    public Opponent Opponent_;
    public GameObject Board_obj;

    public BattleParameters(int width, int height,
                            List<Vector2Int> objectiveCoords,
                            Vector2Int player_deployment_start, Vector2Int player_deployment_end,
                            Vector2Int enemy_deployment_start, Vector2Int enemy_deployment_end,
                            Opponent opponent,
                            GameObject board_obj)
    {
        BoardWidth = width; BoardHeight = height;
        ObjectiveCoords = objectiveCoords;
        Player_deployment_start = player_deployment_start; Player_deployment_end = player_deployment_end;
        Enemy_deployment_start = enemy_deployment_start; Enemy_deployment_end = enemy_deployment_end;
        Opponent_ = opponent;
        Board_obj = board_obj;
    }
}

//Responsible for starting battles & stuff
public class BattleManager : MonoBehaviour
{
    #region Singleton
    public static BattleManager Instance;
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

    


    public IEnumerator StartBattle(BattleParameters parameters)
    {
        yield return new WaitForSeconds(Delay * Time.fixedDeltaTime);

        #region Creating the battlefield
        yield return Action_CreateBattlefield.CreateBattlefield(
            parameters.BoardWidth, parameters.BoardHeight,
            parameters.ObjectiveCoords,
            parameters.Player_deployment_start, parameters.Player_deployment_end,
            parameters.Enemy_deployment_start, parameters.Enemy_deployment_end,
            parameters.Board_obj);
        #endregion

        #region Placing enemy units
        yield return EnemyManager.Instance.StartBattleWithOpponent(parameters.Opponent_);
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

    }

    void Update()
    {
        
    }
}
