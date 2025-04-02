using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public int StarterUnitsAmount;

    public List<Unit> Army = new List<Unit>();
    public List<Unit> Army_NotPlaced = new List<Unit>();
    public List<Unit> Army_Placed = new List<Unit>();

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
        ScoreManager.Instance.TurnEvent.AddListener(DeployNewplayerU);
    }
    void OnDisable()
    {
        ScoreManager.Instance.TurnEvent.RemoveListener(DeployNewplayerU);
    }

    //Gets the whole army back into the NotPlaced category
    public void ResetArmy()
    {
        List<Unit> randArmy = Army;
        for (int i = 0; i < randArmy.Count; i++)
        {
            Unit f = randArmy[i];
            int randI = Random.Range(0, randArmy.Count);

            randArmy[i] = randArmy[randI];
            randArmy[randI] = f;
        }

        Army_NotPlaced.AddRange(randArmy);
        Army_Placed.Clear();
    }

    public void RecordPlacedUnit(Unit unit)
    {
        Debug.Log("Placed player " + unit.UnitName + ", removing from NotPlaced");
        Army_NotPlaced.Remove(unit);
        Army_Placed.Add(unit);
    }

    public void DeployPlayerUnits()
    {
        List<Unit> toDeploy = new List<Unit>();
        int totalAm = 0;
        for (int i = 0; i < StarterUnitsAmount && i < Army_NotPlaced.Count; i++) 
        {
            toDeploy.Add(Army_NotPlaced[i]);
            totalAm++;
        }

        ActionParameters parameters = new ActionParameters(
                    GameManager.ActionType.Deploy, toDeploy, null, BoardManager.Instance.PlayerDeploymentZone, null, totalAm);
        StartCoroutine(GameManager.Instance.Action(parameters));

        foreach (var u in toDeploy) { RecordPlacedUnit(u); }
    }

    public void DeployNewplayerU(ScoreManager.Side side)
    {
        if (side == ScoreManager.Side.Enemy || ScoreManager.Instance.CurRound < 1) return;
        if (Army_NotPlaced.Count <= 0) return;

        List<Unit> newUnit = new List<Unit> { Army_NotPlaced[0] };
        ActionParameters parameters = new ActionParameters(
                    GameManager.ActionType.Deploy, newUnit, null, BoardManager.Instance.PlayerDeploymentZone, null, 1);
        StartCoroutine(GameManager.Instance.Action(parameters));

        RecordPlacedUnit(Army_NotPlaced[0]);
    }

    //Adds selected unit into player's total army
    public void AddUnit(Unit unit)
    {
        Army.Add(unit);
    }

    //Removes the unit from player's total army
    public void RemoveUnit(Unit unit) 
    {
        if (Army.Count > 0 && Army.Contains(unit)) { Army.Remove(unit); } 
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
