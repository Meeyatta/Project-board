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

    [SerializeReference] public List<Ability> AppliedAbilities = new List<Ability>();

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

    private void Start()
    {
        StartCoroutine(CoverAbilitiesTracking());
    }

    #region Adding abilities to unit depending on what covers it stands
    void AddAbilitiesToUnitOnCover(Unit u)
    {
        if (ShouldDebug) Debug.Log("-------------------------------");
        if (ShouldDebug) Debug.Log("Debugging cover for: " + u.name);

        List<Vector2Int> positions = BoardManager.Instance.Get_UnitPositions(u);
        foreach (var pos in positions)
        {
            CoverType posCovT = BoardManager.Instance.Board[pos.x].Cells[pos.y].CoveredBy;
            if (ShouldDebug) Debug.Log("Checking " + pos + " - covered by " + posCovT);

            if (posCovT != CoverType.None)
            {
                Cover c = GetCover(posCovT);
                foreach (var a in c.AppliedAbilities)
                {
                    Ability newAb = a;
                    AbilityInstance abilityInstance = new AbilityInstance(newAb, null, u);
                    Origin_CoverUnderneath og = new Origin_CoverUnderneath(abilityInstance, pos, posCovT);
                    abilityInstance.Origin_ = og;

                    u.Ability_Add(abilityInstance); 
                    //if (ShouldDebug) Debug.Log("Applying ability \" " + a + " \" to " + u.name + " on " + pos);

                    //Ability newAb = a;
                    //newAb.Description = "Ability applied by cover manager";

                    //Origin_CoverUnderneath origin = new Origin_CoverUnderneath(newAb, pos, c.Type_);
                    //newAb.Origin_ = origin;

                    //newAb.Owner = u;

                    //if (!u.HasAbility(newAb)) { Debug.Log("Adding " + newAb.aName + " to " + u.name); u.Ability_Add(newAb); }
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
                    u.Ability_Remove(a);
                }
            }
        }
    }
    #endregion

    #region Tracks what abilities covers underneath units should give
    IEnumerator CoverAbilitiesTracking()
    {
        while (true)
        {
            yield return new WaitForSeconds(Time.fixedDeltaTime);

            List<Unit> all = BoardManager.Instance.Get_AllUnitsOnBoard();
            foreach (var u in all)
            {
                AddAbilitiesToUnitOnCover(u);
                RemoveAbilitiesOfCover(u);
            }
        }
        
    }
    #endregion

    void FixedUpdate()
    {


    }
}
