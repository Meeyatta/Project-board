using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System;

#region Functionality:
/*
    void Build() - Rebuilds all the cells both in the table and in the scene. Doing so sets all cells as unoccupied, 
        must copy the table and Cells object to keep the changes
    void Print() - Prints current board and units on it in the console

    List<Vector2Int> Get_UnitPositions(Unit unit) - takes a Unit and return's it's position on the board
    Vector3? BoardToWorldPosition(List<Vector2Int> poss) - Returns the Vector3 position of a cell under coordinates
    List<Unit> Get_AllUnitsOnBoard() - Returns a list of all units on board cells

    Vector2Int CellClosestToPosition(Vector3 hitPosition) - Returns a singular cell closest to the used Vector3
    Vector2Int CursorToCellPosition() - Returns the cell under the player's cursor

    List<Vector2Int> SingleCellToUnitPositions(Unit unit, Vector2Int cell) - Determines how unit is going to fit according to the single cell 
    List<Vector2Int> ClosestUnitPosToCursor(Unit unit) - Returns the positions of the space where unit can be placed closest to the cursor

    Vector2Int WorldToBoardPosition(Vector3 pos) - Converts Vector3 position to a position on the board

    bool IsInBounds(Vector2Int v) - Returns true if the position is within bounds of the board
    bool AreInBounds(List<Vector2Int> newPoss) - Returns true if all the positions are within bounds of the board

    bool AreAnyOccupied(List<Vector2Int> newPoss) - Returns true if all cells are unoccupied

    bool IsOnBoard(Unit u) - Returns true if unit is on board

    List<Unit> Get_AllUnitsWithKeyword(Keyword keyword) - returns a list of units with specified keyword
    List<Unit> Get_AllUnitsWithAbilities(List<Unit.Ability> abilities) - Returns a list of units with specified abilities

    List<Unit> Get_UnitsInRange(Unit u, int r) - returns list of units within "r" cells of the "u" unit
    List<Unit> Get_UnitsWithKeywordsInRange - Returns list of units within "r" cells of the "u" unit what have all "keywords" keywords
    
    Vector2Int Get_CenterOfZone(List<Vector2Int> zone) - Finds the coordinate closest to the center
 */
#endregion

public class Sorter
{
    public static List<Vector2Int> QuickSort(List<Vector2Int> og)
    {
        List<Vector2Int> temp = new List<Vector2Int>(); temp.AddRange(og);
        QuickSortHelper(temp, 0, temp.Count - 1);

        return temp;
    }

    private static void QuickSortHelper(List<Vector2Int> temp, int low, int high)
    {
        if (low < high)
        {
            int pivotIndex = Partition(temp, low, high);
            QuickSortHelper(temp, low, pivotIndex - 1);
            QuickSortHelper(temp, pivotIndex + 1, high);
        }
    }

    private static int Partition(List<Vector2Int> temp, int low, int high)
    {
        Vector2Int pivot = temp[high];
        int i = low - 1;

        for (int j = low; j < high; j++)
        {
            // Compare first by y, then by x if y's are equal
            if (temp[j].y < pivot.y || (temp[j].y == pivot.y && temp[j].x < pivot.x))
            {
                i++;
                // Swap temp[i] and temp[j]
                Swap(temp, i, j);
            }
        }

        // Swap the pivot element with temp[i + 1]
        Swap(temp, i + 1, high);
        return i + 1;
    }

    private static void Swap(List<Vector2Int> temp, int i, int j)
    {
        Vector2Int tempValue = temp[i];
        temp[i] = temp[j];
        temp[j] = tempValue;
    }
}

public class BoardManager : MonoBehaviour
{
    public LayerMask CellMask;

    public float DefaultY;
    public float CellSize;
    public float InBetweenSpace;
    public int Get_Width() { return Board.Count(); }
    public int Get_Height() { return Board[0].Cells.Count(); }

    #region DeploymentZones for player and current enemy
    [HideInInspector] public Vector2Int PlayerDeployment_start; [HideInInspector] public Vector2Int PlayerDeployment_end;
    [HideInInspector] public Vector2Int EnemyDeployment_start;[HideInInspector] public Vector2Int EnemyDeployment_end;
    #endregion

    public GameObject CellsObj;
    public CellsHolder CHolder;

    public GameObject BoardCellObj;
    public GameObject CurBoard_obj;
    public List<Column> Board;

    public bool ShouldDebug;
    public static BoardManager Instance;
    #region Events 
    void Start()
    {
        
    }
    void OnDisable()
    {
        
    }
    #endregion Events

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
    private void Awake()
    {       
        Singleton();
        CHolder = FindObjectOfType<CellsHolder>();
    }

    /*
        Rebuilds all the cells both in the table and in the scene. Doing so sets all cells as unoccupied,
        (must copy the table and Cells object to keep the changes)
    */

    public IEnumerator Build(int width, int height,
        List<Vector2Int> objectivePositions,
        Vector2Int player_deployment_start, Vector2Int player_deployment_end,
        Vector2Int enemy_deployment_start, Vector2Int enemy_deployment_end,
        Vector3 cellsPosition,
        GameObject boardObject, Vector3 boardPosition)
    {
        yield return new WaitForSeconds(1);

        #region Set up
        PlayerDeployment_start = player_deployment_start; PlayerDeployment_end = player_deployment_end;
        EnemyDeployment_start = enemy_deployment_start; EnemyDeployment_end = enemy_deployment_end;

        int middle_x = Mathf.FloorToInt(width / 2);
        int middle_y = Mathf.FloorToInt(height / 2);
        #endregion

        #region Destroy all previous cell objects
        foreach (Transform t in CellsObj.transform)
        {
            BoardManager_ObjPools.Instance.DestroyToPool(t.gameObject);
        }
        #endregion

        #region Clear all cells in the script
        Board.Clear();
        #endregion

        #region Create cells 
        boardObject.transform.position = CellsObj.transform.parent.TransformPoint(boardPosition);
        CellsObj.transform.position = CellsObj.transform.parent.TransformPoint(cellsPosition);
        //Debug.Log("CellsObj position " + CellsObj.transform.position);
        Vector3 coordsToPos(int x, int y)
        {
            int relative_x = x - middle_x;
            int relative_y = y - middle_y;

            float newX = -relative_x * InBetweenSpace;
            float newY = relative_y * InBetweenSpace;

            //Debug.Log("Creating a cell at " + new Vector3(newX, DefaultY, newY));
            return CellsObj.transform.TransformPoint(new Vector3(newX, DefaultY, newY));
        }

        for (int x = 0; x < width; x++)
        {
            Column column = new Column();
            column.Cells = new List<Cell>();

            for (int y = 0; y < height; y++)
            {
                Cell cell = new Cell();
                cell.Coordinates = new Vector2Int(x, y);
                cell.Position = coordsToPos(x, y);
                cell.CoveredBy = CoverType.None;

                BoardCell bc = BoardManager_ObjPools.Instance.InstantiateFromPool("cell", cell.Position, Quaternion.identity).GetComponent<BoardCell>();
                bc.Coordinates = cell.Coordinates;

                column.Cells.Add(cell);
                //yield return new WaitForSeconds(0.5f);
            }
            Board.Add(column);
        }
        #endregion

        yield return new WaitForSeconds(Time.fixedDeltaTime);

        #region Set up player and enemy zones

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                #region Player 
                if (x >= player_deployment_start.x && x <= player_deployment_end.x &&
                    y >= player_deployment_start.y && y <= player_deployment_end.y)
                {
                    Board[x].Cells[y].Tags.Add(CellTag.PlayerDeploymentZone);
                }
                #endregion

                #region Enemy 
                if (x >= enemy_deployment_start.x && x <= enemy_deployment_end.x &&
                    y >= enemy_deployment_start.y && y <= enemy_deployment_end.y)
                {
                    Board[x].Cells[y].Tags.Add(CellTag.EnemyDeploymentZone);
                }
                #endregion
            }
        }

        #endregion

        yield return new WaitForSeconds(Time.fixedDeltaTime);

        #region Deploy objectives
        if (objectivePositions == null || objectivePositions.Count == 0) yield break;
        for (int i = 0; i < objectivePositions.Count; i++)
        {
            GameObject objective = BattleManager.Instance.Objective_Obj;

            List<Vector2Int> objPos = new List<Vector2Int> { objectivePositions[i] };
            Debug.Log("Placed objective at " + objectivePositions[i]);
            yield return Action_Create.Create(objective, objPos, -1);
        }
        #endregion

        //Create the board


        yield return null;
    }

    public List<Vector2Int> SortPoss(List<Vector2Int> l)
    {
        List<Vector2Int> temp = l;

        //The sorting algoryth doesn't work for a single element, so this needs to be done
        if (temp.Count <= 1) { return temp; }

        return Sorter.QuickSort(temp); ;
    }

    //Prints current board and units on it in the console
    public void Print()
    {
        Debug.Log("-----------------------------");
        for (int y = 0; y < Get_Height(); y++)
        {
            string line = "|";
            for (int x = 0; x < Get_Width(); x++)
            {
                if (Board[x].Cells[y].CurUnit != null)
                {
                    line += "O|";
                }
                else
                {
                    line += "_|";
                }
            }
            Debug.Log(y + ")" + line);
        }
    }

    //Takes a Unit and return's it's position on the board
    public List<Vector2Int> Get_UnitPositions(Unit unit)
    {
        List<Vector2Int> poss = new List<Vector2Int>();
        foreach (Column co in Board)
        {
            foreach (Cell ce in co.Cells)
            {
                if (ce.CurUnit == unit) { poss.Add(ce.Coordinates); }
            }
        }
        if (poss.Count == 0) { Debug.LogWarning("WARNING: UNIT '" + unit.UnitName + "' NOT FOUND"); return null; }
        return poss;
    }

    //Returns the Vector3 position of a cell under coordinates
    public Vector3? BoardToWorldPosition(List<Vector2Int> poss)
    {
        Vector3 newP = Vector3.zero;
        if (poss == null || poss.Count <= 0) { return null; }
        foreach (Vector2Int v in poss)
        {
            if (!IsInBounds(v)) return null;


            newP.x += Board[v.x].Cells[v.y].Position.x;
            newP.y += Board[v.x].Cells[v.y].Position.y;
            newP.z += Board[v.x].Cells[v.y].Position.z;
        }

        newP.x /= poss.Count; newP.y /= poss.Count; newP.z /= poss.Count;

        return newP;

    }

    //Returns a list of all units on board cells
    public List<Unit> Get_AllUnitsOnBoard()
    {
        List<Unit> units = new List<Unit>();
        foreach (var v in Board)
        {
            foreach (var c in v.Cells)
            {
                if (c.CurUnit != null) { units.Add(c.CurUnit); }
            }
        }

        return units;
    }

    //Returns a singular cell closest to the used Vector3
    Vector2Int CellClosestToPosition(Vector3 hitPosition)
    {
        Vector2Int res = new Vector2Int(0, 0);
        float minDist = Mathf.Infinity;
        for (int x = 0; x < Board.Count; x++)
        {
            for (int y = 0; y < Board[x].Cells.Count; y++)
            {
                if (Board[x].Cells[y].CurUnit != null) { continue; }
                if (Vector3.Distance(Board[x].Cells[y].Position /*+ transform.position*/, hitPosition) < minDist) 
                { 
                    minDist = Vector3.Distance(Board[x].Cells[y].Position /*+ transform.position*/, hitPosition); 
                    res = new Vector2Int(x, y); 
                }
            }
        }
        return res;
    }

    //Determines how unit is going to fit according to the single cell provided
    public List<Vector2Int> SingleCellToUnitPositions(Unit unit, Vector2Int cell)
    {
        List<Vector2Int> relativePoss = new List<Vector2Int>();
        foreach (var pos in unit.Size.Positions)
        {         
            relativePoss.Add(cell + pos);
            bool areAll = AreInBounds(relativePoss);

            //If all of these coordinates are within a border, return  these positions
            if (areAll)
            {
                return relativePoss;
            }
        }

        return null;
    }

    //Returns the positions of the space where unit can be placed closest to the cursor
    public List<Vector2Int> ClosestUnitPosToCursor(Unit unit)
    {
        Vector2Int single = ClickManager.Instance.PointedAtCoords;

        return SingleCellToUnitPositions(unit, single);
    }

    //Converts Vector3 position to a position on the board
    public Vector2Int WorldToBoardPosition(Vector3 pos)
    {
        float minDist = Mathf.Infinity; float maxDist = 5;
        BoardCell cRes = null;
        foreach (var c in CHolder.Cells)
        {
            if (Vector3.Distance(c.gameObject.transform.position, pos) < minDist)
            {
                minDist = Vector3.Distance(c.gameObject.transform.position, pos);
                cRes = c;
            }
        }

        if (minDist < maxDist)
        {
            return cRes.Coordinates;
        }
        else
        {
            return Vector2Int.zero;
        }

    }

    //Places unit onto cells under the positions (So doesn't change the position the unit is currently occupying)
    public IEnumerator PlaceUnit(Unit unit, List<Vector2Int> newPos)
    {
        if (newPos == null || newPos.Count <= 0) yield break;

        List<Vector2Int> s = SortPoss(newPos);
        foreach (Vector2Int v in s)
        {
            Board[v.x].Cells[v.y].CurUnit = unit;
        }

        //Debug.Log("Placing unit model to " + BoardToWorldPosition(s).Value + unit.ModelOffset + transform.position);
        AudioManager.Instance.Play(SoundName.Step, unit.transform);
        unit.gameObject.transform.position = BoardToWorldPosition(s).Value + unit.ModelOffset;
    }

    //Returns true if the position is within bounds of the board
    public bool IsInBounds(Vector2Int v)
    {
        if (v.x < 0 || v.x >= Board.Count) { return false; }                       //Changed ">=" to ">", might lead to some errors
        if (v.y < 0 || v.y >= Board[v.x].Cells.Count) { return false; }
    
        return true;
    }

    //Returns true if all the positions are within bounds of the board
    public bool AreInBounds(List<Vector2Int> newPoss)
    {
        foreach (var v in newPoss)
        {
            if (!IsInBounds(v)) { return false; }
        }
        return true;
    }
    //Returns true if all cells are unoccupied
    public bool AreAnyOccupied(List<Vector2Int> newPoss)
    {
        foreach (var v in newPoss)
        {
            if (Board[v.x].Cells[v.y].CurUnit != null)
            { 
                //Debug.Log(v.x + " " + v.y + " is occupied by " + Board[v.x].Cells[v.y].CurUnit.UnitName); 
                return true; 
            }
        }

        return false;
    }
    //Returns true if unit is on board
    public bool IsOnBoard(Unit u)
    {
        foreach(var i in Board)
        {
            foreach (var c in i.Cells)
            {
                if (c.CurUnit == u)
                {
                    return true;
                }
            }
        }
        return false;
    }
    //Returns a list of units with specified abilities
    public List<Unit> Get_AllUnitsOnBoardWithAbilities(List<Ability_Name> abilities)
    {
        List<Unit> units = new List<Unit>();

        foreach (var i in Board)
        {
            foreach (var c in i.Cells)
            {
                if (c == null || c.CurUnit == null) continue;

                bool All = true;
                foreach (var a in abilities)
                {
                    if (!c.CurUnit.HasAbility(a)) { All = false;break; }
                }
                if (All) { units.Add(c.CurUnit); }
            }
        }

        return units;
    }

    //Returns a list of units with specified keyword
    public List<Unit> Get_AllUnitsWithKeywords(List<Keyword> keywords)
    {
        List<Unit> units = new List<Unit>();

        foreach (var i in Board)
        {
            foreach (var c in i.Cells)
            {
                if (c == null || c.CurUnit == null) continue;

                if (c.CurUnit.CurKeywords.Intersect<Keyword>(keywords).Any() && !units.Contains(c.CurUnit))
                {
                    units.Add(c.CurUnit);
                }
            }
        }

        return units;
    }

    //Returns list of units within "r" cells of the "u" unit
    public List<Unit> Get_UnitsInRange(Unit u, int r)
    {
        List<Unit> res = new List<Unit>();
        foreach (var coords in Get_UnitPositions(u))
        {
            for (int x = Mathf.Max(coords.x - r, 0); x < Mathf.Min(coords.x + r + 1, Get_Width()); x++)
            {
                for (int y = Mathf.Max(coords.y - r, 0); y < Mathf.Min(coords.y + r + 1, Get_Height()); y++)
                {
                    if (x == coords.x && y == coords.y) { continue; }

                    //Debug.Log("Checking coord of: " + x + " " + y);
                    if (Board[x].Cells[y].CurUnit != null && Board[x].Cells[y].CurUnit != u) { res.Add(Board[x].Cells[y].CurUnit); }
                }
            }
        }

        return res;
    }
    
    //Returns list of units within "r" cells of the "u" unit what have all "keywords" keywords
    public List<Unit> Get_UnitsWithKeywordsInRange(Unit u, int r, List<Keyword> keywords)
    {
        List<Unit> res = new List<Unit>();
        foreach (var coords in Get_UnitPositions(u))
        {
            for (int x = Mathf.Max(coords.x - r, 0); x < Mathf.Min(coords.x + r + 1, Get_Width()); x++)
            {
                for (int y = Mathf.Max(coords.y - r, 0); y < Mathf.Min(coords.y + r + 1, Get_Height()); y++)
                {
                    if (x == coords.x && y == coords.y) { continue; }

                    //Debug.Log("Checking coord of: " + x + " " + y);
                    if (Board[x].Cells[y].CurUnit != null && Board[x].Cells[y].CurUnit != u
                        && Board[x].Cells[y].CurUnit.CurKeywords.Intersect<Keyword>(keywords).Any()) 
                    { res.Add(Board[x].Cells[y].CurUnit); }
                }
            }
        }

        return res;
    }

    //Finds the coordinate closest to the center
    public Vector2Int Get_CenterOfZone(List<Vector2Int> zone)
    {
        if (zone == null || zone.Count == 0) { Debug.LogError("Get_CenterOfZone received an empty zone"); return Vector2Int.zero; }
        if (zone.Count == 1) return zone[0];

        Vector2 total = Vector2.zero; foreach (var v in zone) { total.x += v.x; total.y += v.y; }
        Vector2 avg = new Vector2(total.x / zone.Count, total.y / zone.Count);

        float minDist = Mathf.Infinity; Vector2Int closest = zone[0];
        foreach (var v in zone) { if (Vector2.Distance(v, avg) <= minDist) {closest = v; minDist = Vector2.Distance(v, avg); } }

        return closest;
    }
    private void Update()
    {
        //Debug.Log("CellsObj position " + CellsObj.transform.position);

        if (Input.GetKeyDown("p")) { Print(); }

        //if (Input.GetKeyDown("b")) { Build(); }

        foreach (var l in Board)
        {
            foreach (var c in l.Cells)
            {
                if (c.Tags.Contains(CellTag.PlayerDeploymentZone)) { Debug.DrawRay(c.Position, Vector3.up, Color.blue); }
                if (c.Tags.Contains(CellTag.EnemyDeploymentZone)) { Debug.DrawRay(c.Position, Vector3.up, Color.red); }
            }
        }

    }
    
}
