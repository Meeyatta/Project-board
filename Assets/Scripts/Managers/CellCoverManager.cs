using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Covering 
{
    None,
    Water, //Applies "Fireproof"
    Oil,   //Applies "Flammable" and "Slippery"
    
};

public class CellCoverManager : MonoBehaviour
{
    #region Singleton
    public static CellCoverManager Instance;
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
    #endregion

    void FixedUpdate()
    {
        List<Unit> list = BoardManager.Instance.Get_AllUnitsOnBoard();

        foreach (var u in list)
        {
            HashSet<Covering> covers = new HashSet<Covering>();
            foreach (var pos in BoardManager.Instance.Get_UnitPositions(u))
            {
                covers.Add(BoardManager.Instance.Board[pos.x].Cells[pos.y].CoveredBy);
            }

            #region Checks for coverings on cells occupied by the unit, adds abilities appropriately
            #region Water
            if (covers.Contains(Covering.Water)) 
            { u.CellCoverAbilities.Add(Ability.Resistant_Fire);  }
            else if (u.CellCoverAbilities.Contains(Ability.Resistant_Fire)) 
            { u.CellCoverAbilities.Remove(Ability.Resistant_Fire); }
            #endregion
            #region Oil
            if (covers.Contains(Covering.Oil)) 
            { u.CellCoverAbilities.Add(Ability.Vulnerable_Fire); u.CellCoverAbilities.Add(Ability.Slippery); }
            else if (u.CellCoverAbilities.Contains(Ability.Vulnerable_Fire) && u.CellCoverAbilities.Contains(Ability.Slippery)) 
            { u.CellCoverAbilities.Remove(Ability.Vulnerable_Fire); u.CellCoverAbilities.Remove(Ability.Slippery); }
            #endregion

            #endregion
        }


    }
}
