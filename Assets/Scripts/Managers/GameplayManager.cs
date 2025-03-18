using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameplayManager : MonoBehaviour
{
    #region Singleton
    public static GameplayManager Instance;
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

    public UnityEvent<List<Unit>> UnitPlacementEvent;
    public void PlaceUnitsAtStartOfTurn(ScoreManager.Side side)
    {
        if (ScoreManager.Instance.CurRound > 1 ) return;
        Debug.Log("PlacingUnit");

        if (side == ScoreManager.Side.Player)
        {
            List<Unit> notP = ResourceManager.Instance.PlayerArmy_NotPlaced;
            if (notP.Count <= 0) { return; }

            ActionParameters parameters = new ActionParameters(
                    GameManager.ActionType.Deploy, notP, null, BoardManager.Instance.PlayerDeploymentZone, null, 1);
            UnitPlacementEvent.Invoke(notP);
            StartCoroutine(GameManager.Instance.Action(parameters));
        }
        else
        {
            List<Unit> notP = EnemyManager.Instance.Army_NotPlaced;
            if (notP.Count <= 0) { return; }

            ActionParameters parameters = new ActionParameters(
                    GameManager.ActionType.Deploy, notP, null, BoardManager.Instance.EnemyDeploymentZone, null, 1);
            UnitPlacementEvent.Invoke(notP);
            StartCoroutine(GameManager.Instance.Action(parameters));
        }
    }

    public void PlaceStarterEnemies(List<Unit> enemyUnitsToDeploy, int enemyStarterAmount)
    {
        ActionParameters parameters = new ActionParameters(
                    GameManager.ActionType.Deploy, enemyUnitsToDeploy, null, BoardManager.Instance.EnemyDeploymentZone, null, enemyStarterAmount);
        UnitPlacementEvent.Invoke(enemyUnitsToDeploy);
        StartCoroutine(GameManager.Instance.Action(parameters));
    }
    public void PlaceStarterPlayerUnits(List<Unit> playerUnitsToDeploy, int playerStarterAmount)
    {
        ActionParameters parameters = new ActionParameters(
                    GameManager.ActionType.Deploy, playerUnitsToDeploy, null, BoardManager.Instance.PlayerDeploymentZone, null, playerStarterAmount);
        UnitPlacementEvent.Invoke(playerUnitsToDeploy);
        StartCoroutine(GameManager.Instance.Action(parameters));
    }
    public void MakeBattlefield(List<Vector2Int> objective_Positions)
    {
        #region Place objectives
        foreach (var p in objective_Positions)
        {
            List<Vector2Int> pos = new List<Vector2Int> { p };
            ActionParameters paramets = new ActionParameters(
                        GameManager.ActionType.Create, null, null, pos, Objective_Obj, 0);
            StartCoroutine( GameManager.Instance.Action(paramets));
        }

        #endregion
    }
    IEnumerator StartBattle()
    {
        yield return new WaitForSeconds(Delay * Time.deltaTime);

        PlaceStarterEnemies(EnemyManager.Instance.StarterArmy, EnemyManager.Instance.EnemyStarterAmount);
        PlaceStarterPlayerUnits(ResourceManager.Instance.StarterArmy, ResourceManager.Instance.PlayerStarterUnitsAmount);

        MakeBattlefield(Objective_Positions);

    }
    void Start()
    {
        StartCoroutine(StartBattle());
        ScoreManager.Instance.TurnEvent.AddListener(PlaceUnitsAtStartOfTurn);
    }
    private void OnDisable()
    {
        ScoreManager.Instance.TurnEvent.RemoveListener(PlaceUnitsAtStartOfTurn);
    }
    void Update()
    {
        
    }


}
