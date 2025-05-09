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
    public int Width;
    public int Height;

    #region DeploymentZones - 2 element lists where first - top-left point, second - bottom-right point
    public List<Vector2Int> PlayerDeploymentZone = new List<Vector2Int> { new Vector2Int(4, 12), new Vector2Int(12, 14) };
    public List<Vector2Int> EnemyDeploymentZone = new List<Vector2Int> { new Vector2Int(4, 2), new Vector2Int(12, 4) };

    #endregion

    public GameObject CellsObj;
    public GameObject BoardCellObj;
    public List<Column> Board;
    
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
        //Build();
    }

    /*
        Rebuilds all the cells both in the table and in the scene. Doing so sets all cells as unoccupied,
        (must copy the table and Cells object to keep the changes)
    */
    void Build()
    {
      foreach (Transform t in CellsObj.transform)
        {
            Destroy(t.gameObject);
        }
        
        Board = new List<Column>();
        for (int x = 0; x < Width; x++)
        {
            Column column = new Column();
            column.Cells = new List<Cell>();

            for (int y = 0; y < Height; y++)
            {
                #region Setup cells in a script table
                Cell cell = new Cell();
                cell.Coordinates = new Vector2Int(x, y);
                cell.Position = CellsObj.transform.position +
                    new Vector3(
                        InBetweenSpace + x * (InBetweenSpace + CellSize), 
                        DefaultY, 
                        -1 * (InBetweenSpace + y * (InBetweenSpace + CellSize)) );
                cell.CoveredBy = Covering.None;

                #region Check if zone is a player deployment zone
                if (x >= PlayerDeploymentZone[0].x && x<= PlayerDeploymentZone[1].x &&
                    y >= PlayerDeploymentZone[0].y && y <= PlayerDeploymentZone[1].y)
                {
                    cell.Tags.Add(CellTag.PlayerDeploymentZone);
                }
                #endregion

                #region Check if zone is an enemy deployment zone
                if (x >= EnemyDeploymentZone[0].x && x <= EnemyDeploymentZone[1].x &&
                    y >= EnemyDeploymentZone[0].y && y <= EnemyDeploymentZone[1].y)
                {
                    cell.Tags.Add(CellTag.EnemyDeploymentZone);
                }
                #endregion

                column.Cells.Add(cell);
                #endregion

                #region Create cell objects in the world
                BoardCell newCell = Instantiate(BoardCellObj, cell.Position, Quaternion.identity, CellsObj.transform).GetComponent<BoardCell>();
                
                newCell.gameObject.name = "Cell " + cell.Coordinates.ToString();
                newCell.Coordinates = cell.Coordinates;
                #endregion
            }
            Board.Add(column);
        }
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
        for (int y = 0; y < Height; y++)
        {
            string line = "|";
            for (int x = 0; x < Width; x++)
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

        return newP + CellsObj.transform.position + transform.position;

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

    //Returns the cell under the player's cursor
    Vector2Int lastPres = new Vector2Int(-90, -90);
    BoardCell lastBoardCell; GameObject lastCellObj;
    public Vector2Int CursorToCellPosition()
    {
        RaycastHit hit;
        Vector3 vect = Input.mousePosition;
        vect.z = 999999;
        Vector3 cPos = Camera.main.ScreenToWorldPoint(vect);
        Physics.Raycast(Camera.main.transform.position, cPos, out hit, Mathf.Infinity, CellMask);
        Debug.DrawRay(Camera.main.transform.position, cPos, Color.green); 

        if (hit.transform != null)
        {
            if (lastCellObj != hit.transform.gameObject)
            {
                lastBoardCell = hit.transform.gameObject.GetComponent<BoardCell>();
            }
            else
            {

            }
        }

        if (lastBoardCell != null) { return lastBoardCell.Coordinates; }
        
        return lastPres;

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
                if (Vector3.Distance(Board[x].Cells[y].Position + CellsObj.transform.position + transform.position, hitPosition) < minDist) 
                { 
                    minDist = Vector3.Distance(Board[x].Cells[y].Position + CellsObj.transform.position + transform.position, hitPosition); 
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
        Vector2Int single = CursorToCellPosition();

        return SingleCellToUnitPositions(unit, single);
    }

    //Converts Vector3 position to a position on the board
    public Vector2Int WorldToBoardPosition(Vector3 pos)
    {
        for (int xi = 0; xi < Board.Count; xi++)
        {
            for (int yi = 0; yi < Board[xi].Cells.Count; yi++)
            {
                if (Board[xi].Cells[yi].Position + CellsObj.transform.position == pos)
                {                    return new Vector2Int(xi, yi); 
                }
            }
        }

        float fx = (pos.x - CellsObj.transform.position.x - InBetweenSpace + CellsObj.transform.position.x + transform.position.x) / (InBetweenSpace + CellSize); 
        int x = (int) fx;

        float fy = (CellsObj.transform.position.z - pos.z - InBetweenSpace + CellsObj.transform.position.z + transform.position.z) / (InBetweenSpace + CellSize);
        int y = (int) fy;

        //Debug.Log(new Vector2Int(x, y));
        return new Vector2Int(x, y);
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
        unit.gameObject.transform.position = BoardToWorldPosition(s).Value + unit.ModelOffset + transform.position;
    }

    //Returns true if the position is within bounds of the board
    public bool IsInBounds(Vector2Int v)
    {
        if (v.x < 0 || v.x >= Board.Count) { return false; }
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
    public List<Unit> Get_AllUnitsWithAbilities(List<Ability> abilities)
    {
        List<Unit> units = new List<Unit>();

        foreach (var i in Board)
        {
            foreach (var c in i.Cells)
            {
                if (c == null || c.CurUnit == null) continue;

                if (c.CurUnit.CurAbilities.Intersect<Ability>(abilities).Any())
                {
                    units.Add(c.CurUnit);
                }
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
            for (int x = Mathf.Max(coords.x - r, 0); x < Mathf.Min(coords.x + r + 1, Width); x++)
            {
                for (int y = Mathf.Max(coords.y - r, 0); y < Mathf.Min(coords.y + r + 1, Height); y++)
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
            for (int x = Mathf.Max(coords.x - r, 0); x < Mathf.Min(coords.x + r + 1, Width); x++)
            {
                for (int y = Mathf.Max(coords.y - r, 0); y < Mathf.Min(coords.y + r + 1, Height); y++)
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
    private void Update()
    {
        if (Input.GetKeyDown("p")) { Print(); }

        if (Input.GetKeyDown("b")) { Build(); }

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
