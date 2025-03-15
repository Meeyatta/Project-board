using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public int PlayerStarterAmount;

    public List<Unit> EnemyUnitsToDeploy;
    public int EnemyStarterAmount;

    public List<Vector2Int> Objective_Positions;
    public GameObject Objective_Obj;

    public void PlaceEnemies(List<Unit> enemyUnitsToDeploy, int enemyStarterAmount)
    {
        ActionParameters parameters = new ActionParameters(
                    GameManager.ActionType.Deploy, enemyUnitsToDeploy, null, BoardManager.Instance.EnemyDeploymentZone, null, enemyStarterAmount);
        StartCoroutine(GameManager.Instance.Action(parameters));
    }
    public void PlacePlayerUnits(List<Unit> playerUnitsToDeploy, int playerStarterAmount)
    {

        ActionParameters parameters = new ActionParameters(
                    GameManager.ActionType.Deploy, playerUnitsToDeploy, null, BoardManager.Instance.PlayerDeploymentZone, null, playerStarterAmount);
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
    IEnumerator test()
    {
        yield return new WaitForSeconds(3 * Time.deltaTime);

        PlaceEnemies(EnemyUnitsToDeploy, EnemyStarterAmount);
        PlacePlayerUnits(ResourceManager.Instance.PlayerArmy_NotPlaced, PlayerStarterAmount);
        MakeBattlefield(Objective_Positions);

    }
    void Start()
    {
        StartCoroutine(test());
    }

    void Update()
    {
        
    }
}
