using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CoverType 
{
    None,
    Water, //Applies "Resistance: Fire" and "Vulnerable:Frost, Lightning"
    Oil,   //Applies "Vulnerable: Fire" and "Slippery"

};

[System.Serializable]
public class Cover
{
    public string Name;
    public CoverType Type_;
    public EffectManager.Tag EffectManagerTag;
    public List<Ability_Name> AppliedAbilities = new List<Ability_Name>();

    public Cover(string n, CoverType type, EffectManager.Tag tag, List<Ability_Name> l)
    {
        Name = n;
        Type_ = type;
        EffectManagerTag = tag;
        AppliedAbilities = l;
    }
}

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

    public bool ShouldDebug;
    public List<Cover> Covers;
    public Cover GetCover(CoverType type)
    {
        foreach (var v in Covers)
        {
            if (v.Type_ == type) 
            {
                if (ShouldDebug) Debug.Log("Found the cover " + v.Name + " type of " + type);
                return v; 
            }
        }

        if (ShouldDebug) Debug.LogError("Cover of type " + type + " not found");
        return null;
    }

    public void CoverCells(CoverType type, List<Vector2Int> cells)
    {
        foreach (var coords in cells)
        {
            BoardManager.Instance.Board[coords.x].Cells[coords.y].CoveredBy = type;
        }
    }
    void Awake()
    {
        Singleton();
    }

    #region Adding abilities to unit depending on what covers it stands
    void AddAbilitiesToUnitOnCover(Unit u)
    {
        List<Vector2Int> positions = BoardManager.Instance.Get_UnitPositions(u);
        foreach (var pos in positions)
        {
            CoverType posCovT = BoardManager.Instance.Board[pos.x].Cells[pos.y].CoveredBy;

            if (posCovT != CoverType.None)
            {
                Cover c = GetCover(posCovT);
                foreach (var a in c.AppliedAbilities)
                {
                    Origin origin = new Origin_CoverUnderneath(pos, c.Type_);
                    Ability newAb = new Ability(a, "Ability applied by cover manager", origin, u);

                    u.Abilities.Add(newAb);
                }
            }
        }
        
    }
    #endregion

    #region Removes all abilities from cell covers when unit no longer stands on top of appropriate ones
    void RemoveAbilitiesOfCover(Unit u)
    {
        List<Vector2Int> positions = BoardManager.Instance.Get_UnitPositions(u);

        //List<>
        foreach (var a in u.Abilities)
        {
            if (a.Origin_ is Origin_CoverUnderneath origin)
            {
                Vector2Int ability_coord = origin.CellCoord;
                if (BoardManager.Instance.Board[ability_coord.x].Cells[ability_coord.y].CoveredBy != origin.CoverType)
                {

                }
            }
        }
    }
    #endregion

    #region Tracks what abilities covers underneath units should give
    void CoverAbilitiesTracking(List<Unit> all)
    {
        foreach (var u in all)
        {
            AddAbilitiesToUnitOnCover(u);
            RemoveAbilitiesOfCover(u);
        }
    }
    #endregion

    void FixedUpdate()
    {
        List<Unit> all = BoardManager.Instance.Get_AllUnitsOnBoard();

        CoverAbilitiesTracking(all);


    }
}
