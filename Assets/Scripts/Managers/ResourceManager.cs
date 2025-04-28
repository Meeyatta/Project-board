using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public int StarterUnitsAmount;

    public List<Unit> Army = new List<Unit>();

    List<Unit> FullArmy = new List<Unit>();
    public List<Unit> Army_NotPlaced = new List<Unit>();
    public List<Unit> Army_Placed = new List<Unit>();

    [Header("---Assigning for convenience sake---")]
    public Transform PlayerUitsTr;

    #region Singleton
    public static ResourceManager Instance;
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
        Action_NextTurn.eTurnEvent_Functional.AddListener(DeployNewplayerUnit);
    }
    void OnDisable()
    {
        Action_NextTurn.eTurnEvent_Functional.RemoveListener(DeployNewplayerUnit);
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
            Unit u = Instantiate(v.gameObject, new Vector3(255, 0, 0), Quaternion.identity, PlayerUitsTr).GetComponent<Unit>();
            u.SetToPlayer();
            FullArmy.Add(u);
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
    }

    public IEnumerator DeployPlayerUnits()
    {
        ActionParameters parameters = new ActionParameters(
            GameManager.ActionType.DeployPlayerStarters, null, null, null, null, StarterUnitsAmount);
        yield return StartCoroutine(GameManager.Instance.Action(parameters));

        yield return new WaitForSeconds(Time.fixedDeltaTime);

        //List<Unit> toDeploy = new List<Unit>();
        //int totalAm = 0;
        //for (int i = 0; i < StarterUnitsAmount && i < Army_NotPlaced.Count; i++)
        //{
        //    toDeploy.Add(Army_NotPlaced[i]);
        //    totalAm++;
        //}

        //ActionParameters parameters = new ActionParameters(
        //            GameManager.ActionType.Deploy, toDeploy, null, BoardManager.Instance.PlayerDeploymentZone, null, totalAm);
        //yield return StartCoroutine(GameManager.Instance.Action(parameters));

        //foreach (var u in toDeploy) { RecordPlacedUnit(u); }
    }

    public void DeployNewplayerUnit(Side side)
    {
        if (side == Side.Enemy || ScoreManager.Instance.CurRound == 0) return;
        if (Army_NotPlaced.Count <= 0) return;
        EffectManager.Instance.HideAllPlayerMovement(side); // <- Safeguards just in case

        ActionParameters parameters = new ActionParameters(
                    GameManager.ActionType.DeployNew, null, null, null, null, 0);
        StartCoroutine(GameManager.Instance.Action(parameters));
        EffectManager.Instance.HideAllPlayerMovement(side); // <- Safeguards just in case
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
