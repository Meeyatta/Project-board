using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public int PlayerStarterUnitsAmount;
    public List<Unit> StarterArmy = new List<Unit>();
    public List<Unit> PlayerArmy = new List<Unit>();
    public List<Unit> PlayerArmy_NotPlaced = new List<Unit>();
    public List<Unit> PlayerArmy_Placed = new List<Unit>();


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
    }
    #endregion

    //Adds selected unit into player's total army
    public void AddUnit(Unit unit)
    {
        PlayerArmy.Add(unit);
    }

    //Removes the unit from player's total army
    public void RemoveUnit(Unit unit) 
    {
        if (PlayerArmy.Count > 0 && PlayerArmy.Contains(unit)) { PlayerArmy.Remove(unit); } 
    }

    //Gets the whole army back into the NotPlaced category
    public void ResetArmy()
    {
        PlayerArmy_Placed.Clear();

        PlayerArmy = GameManager.Instance.Shuffle<Unit>(PlayerArmy);
        PlayerArmy_NotPlaced = PlayerArmy;
    }
    //Unit is placed from reserve onto the board
    public void PlacedTheUnits(List<Unit> units)
    {

        foreach (var unit in units)
        {
            if (PlayerArmy.Contains(unit))
            {
                Debug.Log(PlayerArmy_NotPlaced.Count);
                PlayerArmy_Placed.Add(unit);
            }
        }

        foreach (Unit u in PlayerArmy_Placed)
        {
            if (PlayerArmy_NotPlaced.Contains(u)) PlayerArmy_NotPlaced.Remove(u);
        }
    }

    void Start()
    {
        ResetArmy();

        List<Unit> AllUnitObjects = new List<Unit>();
        foreach (Unit unit in PlayerArmy) 
        {
            AllUnitObjects.Add(
                Instantiate(unit.gameObject).GetComponent<Unit>());
        }
        GameplayManager.Instance.UnitPlacementEvent.AddListener(PlacedTheUnits);
        
    }
    private void OnDisable()
    {
        GameplayManager.Instance.UnitPlacementEvent.RemoveListener(PlacedTheUnits);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
