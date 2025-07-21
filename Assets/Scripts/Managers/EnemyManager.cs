using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public int StarterUnitsAmount;
    public bool ShouldDebug;

    [Header("---Delays---")]
    public float DeployNewUnit_Before;
    public float DeployNewUnit_After;
    public float ThinkingDelay;
    public float DelayAfterRaisingAUnit;
    public float DelayAfterMovingAUnit;

    [Header("---Read only variables--")]
    public bool HasToDeploy;
    public bool IsPlayingTurn;
    public Unit NextUnit;

    public List<UnitAmount> Army = new List<UnitAmount>();

    List<Unit> FullArmy = new List<Unit>();
    public List<Unit> Army_NotPlaced = new List<Unit>();
    public List<Unit> Army_Placed = new List<Unit>();

    [Header("---Assigning for convenience sake---")]
    public Transform EnemyUitsTr;

    const string IsRaisedStr = "isRaised";

    #region Singleton
    public static EnemyManager Instance;
    void Singleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ResetArmy();
    }
    void Awake()
    {
        Singleton();

    }
    #endregion

    void Start()
    {
        Action_NextTurn.E_Turn_Functional.AddListener(EnemyTurn_Handler);
    }

    void OnDisable()
    {
        Action_NextTurn.E_Turn_Functional.RemoveListener(EnemyTurn_Handler);
    }

    Coroutine cEnemyTurn; float nextEnemyTurnTime = 0.1f; public float cooldown = 4;
    void EnemyTurn_Handler(Side s)
    {
        if (cEnemyTurn == null && !IsPlayingTurn && Time.time > nextEnemyTurnTime) 
        {
            nextEnemyTurnTime = Time.time + cooldown * Time.fixedDeltaTime * 50;
            cEnemyTurn = StartCoroutine(EnemyTurn()); 
        }

    }

    void EndEnemyTurn()
    {
        if (BattleStatsManager.Instance.CurRound > 0) HasToDeploy = true;
        IsPlayingTurn = false;
        if (cEnemyTurn != null) StopCoroutine(cEnemyTurn);
        cEnemyTurn = null;
        if (ShouldDebug) Debug.Log("Ended the enemy turn");
    }

    IEnumerator EnemyTurn()
    {
        if (BattleStatsManager.Instance.CurTurn == Side.Player) { EndEnemyTurn(); yield break; }
        yield return new WaitForSeconds(Time.fixedDeltaTime);
        if (BattleStatsManager.Instance.CurTurn == Side.Player) { EndEnemyTurn(); yield break; }

        IsPlayingTurn = true;
        if (ShouldDebug) Debug.Log("Started enemy turn");

        if (BattleStatsManager.Instance.CurRound > 0)
        {
            if (BattleStatsManager.Instance.CurTurn == Side.Player) { EndEnemyTurn(); yield break; }
            if (ShouldDebug) Debug.Log("Thinking");
            yield return new WaitForSeconds(ThinkingDelay);

            if (BattleStatsManager.Instance.CurTurn == Side.Player) { EndEnemyTurn(); yield break; }

            //if (DebugBehaviour) Debug.Log("Deploying new enemy unit");  <-- Vestige from when enemy used to place their unit DURING their turn
            //yield return DeployNewEnemy(); //Deploying a new enemy unit

            if (BattleStatsManager.Instance.CurTurn == Side.Player) { EndEnemyTurn(); yield break; }

            if (ShouldDebug) Debug.Log("Moving all enemy units");
            yield return MovingAllEnemyUnits(); //Moving all units on the board
        }

        if (ShouldDebug) Debug.Log("initiated the next turn");
        ActionParameters turnParameters = new ActionParameters(GameManager.ActionType.NextTurn, null, null, null, null, 0);
        yield return GameManager.Instance.Action(turnParameters);

        EndEnemyTurn();
    }

    #region For deploying a new enemy unit next turn
    public IEnumerator DeployNewEnemyUnit()
    {
        if (BattleStatsManager.Instance.CurRound <= 1 || Army_NotPlaced.Count <= 0) yield break;
        HasToDeploy = false;
        if (ShouldDebug) Debug.Log("Started DeployNewEnemy");

        CameraManager.Instance.SetCam(CameraManager.Instance.FrontUpper);

        yield return new WaitForSeconds(DeployNewUnit_Before);

        List<Unit> newEnemy = new List<Unit> { Army_NotPlaced[0] };
        ActionParameters parameters = new ActionParameters(
                    GameManager.ActionType.Deploy, newEnemy, null, BoardManager.Instance.EnemyDeploymentZone, null, 1);
        yield return Action_Deployment.Deploy(parameters);

        RecordPlacedUnit(Army_NotPlaced[0]);

        yield return new WaitForSeconds(DeployNewUnit_After);

        if (ShouldDebug) Debug.Log("Ended DeployNewEnemy");
    }
    #endregion

    #region Go through each unit on the board and move them towards objectives
    public IEnumerator MovingAllEnemyUnits()
    {
        if (BattleStatsManager.Instance.CurRound < 1 || Army_Placed.Count <= 0) yield break;

        List<Unit> units = BoardManager.Instance.Get_AllUnitsWithKeywords(new List<Keyword> { Keyword.Enemy });

        List<ActionParameters> MoveActions = new List<ActionParameters>();
        #region Going through each unit one by one and moving them
        foreach (var unit in units)
        {
            #region Raise the unit's model, wait for a couple of seconds, then move them
            unit.Anim.SetBool(IsRaisedStr, true);
            yield return new WaitForSeconds(DelayAfterRaisingAUnit);
            unit.Anim.SetBool(IsRaisedStr, false);
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

            #endregion

            #region Wait while the unit is being moved
            int safeGuard = 10000;
            while (
                ((GameManager.Instance.CurrentAction != null && MoveActions.Contains(GameManager.Instance.CurrentAction.Params)) ||
                (GameManager.Instance.ActionQueue.Count > 0 && MoveActions.Contains(GameManager.Instance.ActionQueue.Peek().Params)))
                 && safeGuard > 0)
            {
                safeGuard--;
                yield return new WaitForSeconds(Time.fixedDeltaTime);
            }
            if (safeGuard <= 0) { Debug.LogError("Safeguard expended, something is very wrong"); }
            #endregion

            yield return new WaitForSeconds(DelayAfterMovingAUnit);
        }
        #endregion

        #region End moving all units and end the turn
        if (ShouldDebug) Debug.Log("Ended moving all enemy units");

        #endregion
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
        if (o != null && o.Count > 0) 
        { 
            if (ShouldDebug) Debug.Log(u.UnitName + " is already within an objective, going to stand still"); 
            return null; }
        #endregion

        #region Find closest, 2nd closest and one other objective
        List<Unit> objectives = BoardManager.Instance.Get_AllUnitsWithKeywords(new List<Keyword> { Keyword.Objective });
        float minD = Mathf.Infinity; float maxD = -1;
        Unit closestObj = null; Unit scndClosestObj = null; Unit otherObj = null;
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

        //One other objective
        foreach (var ob in objectives)
        {
            if (ob != closestObj && ob != scndClosestObj) { otherObj = ob; break; }
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
                //Debug.Log(u.UnitName + " found closest objective doesn't have enough lead - moves to closest");
                return GetPosToPoint(closestObj, u);
            }
        }
        #endregion

        #region Check if 2nd closest objectives has >1 enemy unit lead
        if (scndClosestObj != null)
        {
            List<Unit> pU = BoardManager.Instance.Get_UnitsWithKeywordsInRange(scndClosestObj, 1, new List<Keyword> { Keyword.Player });
            List<Unit> eU = BoardManager.Instance.Get_UnitsWithKeywordsInRange(scndClosestObj, 1, new List<Keyword> { Keyword.Enemy });

            if (eU.Count <= pU.Count)
            {
                //Closest objective doesn't have enough of a lead, go to it
                //Debug.Log(u.UnitName + " found 2nd closest objective doesn't have enough lead - moves to 2nd closest");
                return GetPosToPoint(scndClosestObj, u);
            }
        }
        #endregion

        #region Move towards the oter remaining objective
        if (otherObj != null)
        {
            Debug.Log(u.UnitName + " Moves to the other objective");
            return GetPosToPoint(otherObj, u);
        }
        #endregion

        //Emergency situation, something went wrong
        if (ShouldDebug) Debug.Log(closestObj);
        if (ShouldDebug) Debug.Log(scndClosestObj);
        if (ShouldDebug) Debug.Log("No AI conditions satisfied, something is wrong");
        return null;
    }
    #endregion

    #region ResetArmy() - Gets the whole army back into the NotPlaced category
    public void ResetArmy()
    {
        List<Unit> randArmy = FullArmy;
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
    #endregion

    public void RecordPlacedUnit(Unit unit)
    {
        //Debug.Log("Placed enemy " + unit.UnitName + ", removing from NotPlaced");
        Army_NotPlaced.Remove(unit);
        Army_Placed.Add(unit);
    }

    public IEnumerator InstantiateEnemyUnits()
    {
        foreach (var v in Army)
        {
            for (var i = 0; i < v.Amount; i++)
            {
                Unit u = Instantiate(v.Unit_, new Vector3(255, 0, 0), Quaternion.identity, EnemyUitsTr).GetComponent<Unit>();
                yield return new WaitForSeconds(Time.fixedDeltaTime);
                u.SetToEnemy();
                FullArmy.Add(u);
            }           
        }
    }

    public IEnumerator DeployEnemies()
    {
        List<Unit> toDeploy = new List<Unit>();
        int totalAm = 0;
        for (int i = 0; i < StarterUnitsAmount && i < Army_NotPlaced.Count; i++)
        {
            toDeploy.Add(Army_NotPlaced[i]);
            totalAm++;
        }

        ActionParameters parameters = new ActionParameters(
                    GameManager.ActionType.Deploy, toDeploy, null, BoardManager.Instance.EnemyDeploymentZone, null, totalAm);
        yield return StartCoroutine(GameManager.Instance.Action(parameters));

        foreach (var u in toDeploy) { RecordPlacedUnit(u); }
    }
    
    #region Returns the position the target can move what is the closest to the "objective"
    List<Vector2Int> GetPosToPoint(Unit objective, Unit target)
    {
        Vector3 objPos = BoardManager.Instance.BoardToWorldPosition(BoardManager.Instance.Get_UnitPositions(objective)).Value;
        List<List<List<Vector2Int>>> posMov = Action_Move.Get_PossibleMovement(target);

        float minD = Mathf.Infinity; List<Vector2Int> res = new List<Vector2Int>();

        //Debug.Log(posMov.Count);

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

        //Debug.Log(res.Count);
        return res;
    }
    #endregion

    

    
}
