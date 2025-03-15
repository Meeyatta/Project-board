using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public int PlayerStarterUnitsAmount;

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
        PlayerArmy_NotPlaced = PlayerArmy;
    }
    //Unit is placed from reserve onto the board
    public void PlacedTheUnit(Unit unit)
    {
        PlayerArmy_NotPlaced.Remove(unit);
        PlayerArmy_Placed.Add(unit);
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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
