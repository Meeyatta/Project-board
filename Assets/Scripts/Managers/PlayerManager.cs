using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int StarterUnitsAmount;

    public List<UnitAmount> Army = new List<UnitAmount>();

    List<Unit> FullArmy = new List<Unit>();
    public List<Unit> Army_NotPlaced = new List<Unit>();
    public List<Unit> Army_Placed = new List<Unit>();

    [Header("---Assigning for convenience sake---")]
    public Transform PlayerUitsTr;

    #region Singleton
    public static PlayerManager Instance;
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
        ResetArmy();
    }
    #endregion
    void Start()
    {
        Action_NextTurn.E_Turn_Functional.AddListener(InitiatePlayerTurn);
    }
    void OnDisable()
    {
        Action_NextTurn.E_Turn_Functional.RemoveListener(InitiatePlayerTurn);
    }

    Coroutine DoingPlayerTurn = null;
    public void InitiatePlayerTurn(Side s)
    {
        if (s == Side.Enemy) return;
        if (DoingPlayerTurn == null)
        {
            DoingPlayerTurn = StartCoroutine(PlayerTurn());
        }
    }
    IEnumerator PlayerTurn()
    {
        yield return EnemyManager.Instance.DeployNewEnemyUnit();

        yield return new WaitForSeconds(Time.fixedDeltaTime);

        yield return DeployNewplayerUnit();

        DoingPlayerTurn = null;
    }
    #region Returns a list of shuffled units within a list
    public List<Unit> ShuffleList(List<Unit> list)
    {
        List<Unit> res = new List<Unit>();
        res.AddRange(list);

        for (int i = 0; i < res.Count; i++)
        {
            Unit f = res[i];
            int randI = Random.Range(0, res.Count);

            res[i] = res[randI];
            res[randI] = f;
        }

       
        return res;
    }
    #endregion

    #region Gets the whole army back into the NotPlaced category
    public void ResetArmy()
    {
        List<Unit> randArmy = ShuffleList(FullArmy);

        Army_NotPlaced.AddRange(randArmy);
        Army_Placed.Clear();
    }
    #endregion

    #region Should be called each time we place a new unit on the board
    public void RecordPlacedUnit(Unit unit)
    {
        //Debug.Log("Placed player " + unit.UnitName + ", removing from NotPlaced");
        Army_NotPlaced.Remove(unit);
        Army_Placed.Add(unit);
    }
    #endregion

    public IEnumerator InstantiatePlayerUnits()
    {
        foreach (var v in Army)
        {
            for (var i = 0; i < v.Amount; i++)
            {
                Unit u = Instantiate(v.Unit_, new Vector3(255, 0, 0), Quaternion.identity, PlayerUitsTr).GetComponent<Unit>();
                u.SetToPlayer();
                FullArmy.Add(u);
                yield return new WaitForSeconds(Time.fixedDeltaTime);
            }
                
        }
    }

    public IEnumerator DeployPlayerUnits()
    {
        List<Vector2Int> amount =new List<Vector2Int> { new Vector2Int(StarterUnitsAmount, 1) };
        ActionParameters parameters = new ActionParameters(
            GameManager.ActionType.DeployPlayerStarters, null, null, amount, null, 0);
        yield return Action_DrawPlayerResources.DeployNewResources(parameters);

        yield return new WaitForSeconds(Time.fixedDeltaTime);

    }

    public IEnumerator DeployNewplayerUnit()
    {
        if (BattleStatsManager.Instance.CurTurn == Side.Enemy || BattleStatsManager.Instance.CurRound == 0) yield break;
        if (Army_NotPlaced.Count <= 0) yield break;
        EffectManager.Instance.HideAllPlayerMovement(BattleStatsManager.Instance.CurTurn); // <- Safeguards just in case

        ActionParameters parameters = new ActionParameters(
            GameManager.ActionType.DeployPlayerStarters, null, null, null, null, 0);
        yield return Action_DrawPlayerResources.DeployNewResources(parameters);

        EffectManager.Instance.HideAllPlayerMovement(BattleStatsManager.Instance.CurTurn); // <- Safeguards just in case
    }

    //Adds selected unit into player's total army
    public void AddUnit(Unit unit)
    {
        FullArmy.Add(unit);
        Army_NotPlaced.Add(unit);
    }

    //Removes the unit from player's total army
    public void RemoveUnit(Unit unit) 
    {
        if (FullArmy.Count > 0 && FullArmy.Contains(unit)) { FullArmy.Remove(unit); } 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
