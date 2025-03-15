using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    #region Singleton
    public static EnemyManager Instance;
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

        ScoreManager.Instance.TurnEvent.AddListener(MakeMoves);
    }
    #endregion

    public List<Unit> UnitsToPlace;
    Coroutine CMakingMoves;

    public void MakeMoves(ScoreManager.Side side)
    {
        if (side == ScoreManager.Side.Player) return;

        if (CMakingMoves == null) 
        {
            CMakingMoves = StartCoroutine(MakingMoves());
        }
    }
    /*
        Selects a position to move the unit depending on units within objectives:
        1) Is within objective?
            Yes) Move nowhere
            No) Go to step 2
        2) Does closest objective have >1 enemy-to-player lead?
            Yes) Move towards the second closest objective
            No) Move towards that objective
    */
    List<Vector2Int> DecidePosition(Unit u)
    {
        #region Check if within objective - if yes, stay still
        List<Unit> o = BoardManager.Instance.Get_UnitsWithKeywordsInRange(u, 1, new List<Keyword> { Keyword.Objective });
        if (o != null && o.Count > 0) { return null; }
        #endregion

        #region Find closest and 2nd closest objectives
        List<Unit> objectives = BoardManager.Instance.Get_AllUnitsWithKeywords(new List<Keyword> { Keyword.Objective });
        float minD = Mathf.Infinity; float maxD = -1;
        Unit closest = null; Unit scndClosest = null;
        //Closest
        foreach (var ob in objectives) 
        {
            if (Vector3.Distance(ob.transform.position, u.transform.position) < minD) 
            {
                if (closest != null) { scndClosest = closest; }
                closest = ob; minD = Vector3.Distance(ob.transform.position, u.transform.position);
            }
            if (Vector3.Distance(ob.transform.position, u.transform.position) > maxD)
            {
                maxD = Vector3.Distance(ob.transform.position, u.transform.position);
            }
        }
        //2nd closest
        foreach (var ob in objectives)
        {
            if (Vector3.Distance(ob.transform.position, u.transform.position) > minD 
                && Vector3.Distance(ob.transform.position, u.transform.position) < maxD)
            {
                scndClosest = ob;
            }

        }
        #endregion

        #region Check if closest objectives has >1 enemy unit lead
        if (closest != null)
        {
            List<Unit> pU = BoardManager.Instance.Get_UnitsWithKeywordsInRange(closest, 1, new List<Keyword> { Keyword.Player });
            List<Unit> eU = BoardManager.Instance.Get_UnitsWithKeywordsInRange(closest, 1, new List<Keyword> { Keyword.Enemy });
        }
        #endregion
    }
    //Go through each 
    IEnumerator MakingMoves()
    {
        List<Unit> units = BoardManager.Instance.Get_AllUnitsWithKeywords(new List<Keyword> { Keyword.Enemy });

        foreach (var unit in units)
        {
            //Decide where unit will move
            List<Vector2Int> newPos = DecidePosition(unit);

            #region Move that unit

            #endregion

            yield return new WaitForSeconds(Time.deltaTime);    
        }

        CMakingMoves = null;
    }
}
