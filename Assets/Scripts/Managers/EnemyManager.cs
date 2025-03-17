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

    }
    #endregion
    void Start()
    {
        ScoreManager.Instance.TurnEvent.AddListener(MoveAllEnemyUnits);
    }
    Coroutine CMakingMoves;
    [Header("---Functionality stuff---")]
    public float DelayBeforeMovingUnit;
    public float DelayAfterMovingUnit;
    public float DelayBeforeEndingTurn;

    public void MoveAllEnemyUnits(ScoreManager.Side side)
    {
        if (side == ScoreManager.Side.Player) return;

        if (CMakingMoves == null) 
        {
            CMakingMoves = StartCoroutine(MovingAllEnemyUnits());
        }
    }

    #region Returns the position the target can move what is the closest to the "objective"
    List<Vector2Int> GetPosToPoint(Unit objective, Unit target)
    {
        Vector3 objPos = BoardManager.Instance.BoardToWorldPosition(BoardManager.Instance.Get_UnitPositions(objective)).Value;
        List<List<List<Vector2Int>>> posMov = Action_Move.Get_PossibleMovement(target);

        float minD = Mathf.Infinity; List<Vector2Int> res = new List<Vector2Int>();

        Debug.Log(posMov.Count);

        foreach (var line in posMov)
        {
            foreach (var poss in line)
            {
                Vector3 pp = BoardManager.Instance.BoardToWorldPosition(poss).Value;

                //Debug.Log(Vector3.Distance(pp, objPos));
                if (Vector3.Distance(pp, objPos) < minD)
                {
                    //Debug.Log("Got a position");
                    minD = Vector3.Distance(pp, objPos);
                    res = poss;
                }
            }
        }

        Debug.Log(res.Count);
        return res;
    }
    #endregion

    #region Summarized enemy AI
    /*
        Selects a position to move the unit depending on units within objectives:
        1) Is within objective?
            Yes) Move nowhere
            No) Go to step 2
        2) Does closest objective have >1 enemy-to-player lead?
            Yes) Move towards the second closest objective
            No) Move towards that objective
    */
    #endregion
    #region Goes through enemy AI and decides what target to find
    List<Vector2Int> DecideMovement(Unit u)
    {
        #region Check if within objective - if yes, stay still
        List<Unit> o = BoardManager.Instance.Get_UnitsWithKeywordsInRange(u, 1, new List<Keyword> { Keyword.Objective });
        if (o != null && o.Count > 0) { Debug.Log(u.UnitName + " is already within an objective, going to stand still"); return null; }
        #endregion

        #region Find closest and 2nd closest objectives
        List<Unit> objectives = BoardManager.Instance.Get_AllUnitsWithKeywords(new List<Keyword> { Keyword.Objective });
        float minD = Mathf.Infinity; float maxD = -1;
        Unit closestObj = null; Unit scndClosestObj = null;
        //Closest
        foreach (var ob in objectives) 
        {
            if (Vector3.Distance(ob.transform.position, u.transform.position) < minD) 
            {
                if (closestObj != null) { scndClosestObj = closestObj; }
                closestObj = ob; minD = Vector3.Distance(ob.transform.position, u.transform.position);
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
                scndClosestObj = ob;
            }

        }
        #endregion

        #region Check if closest objectives has >1 enemy unit lead
        if (closestObj != null)
        {
            List<Unit> pU = BoardManager.Instance.Get_UnitsWithKeywordsInRange(closestObj, 1, new List<Keyword> { Keyword.Player });
            List<Unit> eU = BoardManager.Instance.Get_UnitsWithKeywordsInRange(closestObj, 1, new List<Keyword> { Keyword.Enemy });

            if (eU.Count <= pU.Count) 
            {
                //Closest objective doesn't have enough of a lead, go to it
                Debug.Log(u.UnitName + " found closest objective doesn't have nough lead - moves to closest");
                return GetPosToPoint(closestObj, u);
            }
        }
        #endregion

        #region Move towards the second closest objective
        if (scndClosestObj != null)
        {
            Debug.Log(u.UnitName + " Moves to second closest");
            return GetPosToPoint(scndClosestObj, u);
        }
        #endregion

        //Emergency situation, sonmething went wrong
        Debug.Log(closestObj);
        Debug.Log(scndClosestObj);
        Debug.Log("No AI conditions satisfied, something is wrong");
        return null;
    }
    #endregion

    #region Go through each unit on the board and move them towards objectives
    IEnumerator MovingAllEnemyUnits()
    {
        yield return new WaitForSeconds(Time.deltaTime);

        List<Unit> units = BoardManager.Instance.Get_AllUnitsWithKeywords(new List<Keyword> { Keyword.Enemy });

        List<ActionParameters> MoveActions = new List<ActionParameters>();
        #region Going through each unit one by one and moving them
        foreach (var unit in units)
        {
            #region Raise the unit's model, wait for a couple of seconds, then move them
            Vector3 ogPos = unit.gameObject.transform.position;
            unit.transform.position = ogPos + Vector3.up * 1;
            yield return new WaitForSeconds(Time.deltaTime * DelayBeforeMovingUnit);
            //unit.gameObject.transform.position = ogPos;

            yield return new WaitForSeconds(Time.deltaTime);
            #endregion

            #region Move that unit
            List<Vector2Int> newPos = DecideMovement(unit);

            if (newPos != null)
            {
                ActionParameters parameters = new ActionParameters(
                    GameManager.ActionType.Move,
                    new List<Unit> { unit }, null, newPos, null, 0);
                MoveActions.Add(parameters);
                yield return StartCoroutine(GameManager.Instance.Action(parameters));
            }
            yield return new WaitForSeconds(Time.deltaTime * DelayAfterMovingUnit);
            #endregion

            #region Wait while the unit is being moved
            int safeGuard = 10000;
            while (
                ((GameManager.Instance.CurrentAction != null && MoveActions.Contains(GameManager.Instance.CurrentAction.Params)) ||
                (GameManager.Instance.ActionQueue.Count > 0 && MoveActions.Contains(GameManager.Instance.ActionQueue.Peek().Params)))
                 && safeGuard > 0)
            {
                safeGuard--;
                yield return new WaitForSeconds(Time.deltaTime);
            }
            if (safeGuard <= 0) { Debug.LogError("Safeguard expended, something is very wrong"); }
            #endregion

        }
        #endregion

        #region End moving all units and end the turn
        yield return new WaitForSeconds(DelayBeforeEndingTurn * Time.deltaTime);
        Debug.Log("Finished moving all enemy units");
        CMakingMoves = null; 

        ActionParameters turnParameters = new ActionParameters(GameManager.ActionType.NextTurn, null, null, null, null, 0);
        StartCoroutine(GameManager.Instance.Action(turnParameters));
        #endregion
    }
    #endregion
}
