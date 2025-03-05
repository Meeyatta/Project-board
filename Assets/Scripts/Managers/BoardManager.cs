using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

#region Functionality:
/*
    void Build() - Rebuilds all the cells both in the table and in the scene. Doing so sets all cells as unoccupied, 
        must copy the table and Cells object to keep the changes
    void Print() - Prints current board and units on it in the console

    List<Vector2Int> Get_UnitPositions(Unit unit) - takes a Unit and return's it's position on the board
    Vector3? BoardToWorldPosition(List<Vector2Int> poss) - Returns the Vector3 position of a cell under coordinates
    IEnumerator MoveUnit(Unit unit, List<Vector2Int> newPos) - Moves the "unit" to the newPos (If newPos can be moved to)
    List<Unit> Get_AllUnitsOnBoard() - Returns a list of all units on board cells

    Vector2Int CellClosestToPosition(Vector3 hitPosition) - Returns a singular cell closest to the used Vector3
    Vector2Int CursorToCellPosition() - Returns the cell under the player's cursor
    List<Vector2Int> ClosestUnitPosToCursor(Unit unit) - Returns the positions of the space where unit can be placed closest to the cursor

    bool AreCellsOccupied(List<Vector2Int> poss) - Checks if any of the cells are occupied
    Vector2Int WorldToBoardPosition(Vector3 pos) - Converts Vector3 position to a position on the board

    bool IsInBounds(Vector2Int v) - Returns true if the position is within bounds of the board
    bool IsOnBoard(Unit u) - Returns true if unit is on board

    List<Unit> Get_AllUnitsWithKeyword(Unit.Keyword keyword) - returns a list of units with specified keyword
    List<Unit> Get_AllUnitsWithAbilities(List<Unit.Ability> abilities) - Returns a list of units with specified abilities

    List<Unit> Get_UnitsInRange(Unit u, int r) - returns list of units within "r" cells of the "u" unit
 */
#endregion

public class BoardManager : MonoBehaviour
{
    public LayerMask CellMask;

    public float DefaultY;
    public float CellSize;
    public float InBetweenSpace;
    public int Width;
    public int Height;

    public GameObject CellsObj;
    public GameObject BoardCellObj;
    public List<Column> Board;
    
    public static BoardManager Instance;
    Vector2Int lastPres = new Vector2Int(-90, -90);
    #region Events 
    public UnityEvent<Vector2Int> ClickEvent;
    void Start()
    {
        ClickEvent.AddListener(GameManager.Instance.CellClickHandle);
    }
    void OnDisable()
    {
        ClickEvent.RemoveListener(GameManager.Instance.CellClickHandle);
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
                Cell cell = new Cell();
                cell.Coordinates = new Vector2Int(x, y);
                cell.Position = CellsObj.transform.position + new Vector3(InBetweenSpace + x * (InBetweenSpace + CellSize), DefaultY, -1 * (InBetweenSpace + y * (InBetweenSpace + CellSize)) );
                column.Cells.Add(cell);

                BoardCell newCell = Instantiate(BoardCellObj, cell.Position, Quaternion.identity, CellsObj.transform).GetComponent<BoardCell>();
                newCell.gameObject.name = "Cell " + cell.Coordinates.ToString();
                newCell.Coordinates = cell.Coordinates;
            }
            Board.Add(column);
        }
    }

    public List<Vector2Int> SortPoss(List<Vector2Int> l)
    {
        List<Vector2Int> temp = l;

        //The sorting algoryth doesn't work for a single element, so this needs to be done
        if (temp.Count <= 1) { return temp; }

        #region Sorting algorythm
        long justincase = 999999;
        bool needsSorting = true;
        while (needsSorting && justincase > 0)
        {
            needsSorting = false;
            justincase--;
            for (int i = 0; i < temp.Count-1; i++) 
            {
                Vector2Int Cur = temp[i];
                Vector2Int Nex = temp[i + 1];

                if (Cur.y > Nex.y) 
                {
                    needsSorting = true;

                    temp[i] = Nex;
                    temp[i+1] = Cur;
                }
                if (Cur.y == Nex.y && Cur.x > Nex.x)
                {
                    needsSorting = true;

                    temp[i] = Nex;
                    temp[i+1] = Cur;
                }
            }
        }
        #endregion

        return temp;
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
        if (poss.Count <= 0) { return newP; }
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

    //Moves the "unit" to the newPos (If newPos can be moved to)
    public IEnumerator MoveUnit(Unit unit, List<Vector2Int> newPos)
    {
        foreach (Vector2Int v in newPos)
        {
            if (v.x >= Board.Count || v.y >= Board[0].Cells.Count) { Debug.LogError("ERROR: POSITION '" + v + "' OUT OF BOUNDS"); yield break; }
        }

        List<Vector2Int> oldPos = Get_UnitPositions(unit);
        foreach (Vector2Int v in oldPos)
        {
            Board[v.x].Cells[v.y].CurUnit = null;
        }

        yield return StartCoroutine(PlaceUnit(unit, newPos));

        yield break;
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
    public Vector2Int CursorToCellPosition()
    {
        Vector2Int res = Vector2Int.zero;


        Vector3 v = Input.mousePosition;
        v.z = 999999;
        Vector3 cPos = Camera.main.ScreenToWorldPoint(v);
        RaycastHit hit;
        Physics.Raycast(Camera.main.transform.position, cPos, out hit, CellMask);

        if (hit.transform != null)
        {
            if (hit.transform.tag == "Cell")
            {
                res = WorldToBoardPosition(hit.transform.gameObject.transform.position);
            }
            else
            {
                //Debug.Log("Cursor is not on a board " + hit.transform.position);
                //Debug.DrawLine(Camera.main.transform.position, cPos, Color.red);
                res = CellClosestToPosition(hit.point);
            }
        }
        else
        {
            res = lastPres;
        }

        lastPres = res;
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
                if (Vector3.Distance(Board[x].Cells[y].Position, hitPosition) < minDist) 
                { 
                    minDist = Vector3.Distance(Board[x].Cells[y].Position, hitPosition); 
                    res = new Vector2Int(x, y); 
                }
            }
        }
        return res;
    }
    //Returns the positions of the space where unit can be placed closest to the cursor
    public List<Vector2Int> ClosestUnitPosToCursor(Unit unit)
    {
        Vector2Int single = CursorToCellPosition();
        //Go through each cell
        for (int i = 0; i < unit.Size.Positions.Count; i++)
        {
            //Find positions of other cells relative to this cell
            List<Vector2Int> relativePoss = new List<Vector2Int>();
            for (int ii = 0; ii < unit.Size.Positions.Count; ii++)
            {
                //Add this positions to form possible coordinates
                //Debug.Log(ii + " - " + (single + unit.Size.Positions[i] - unit.Size.Positions[ii]));
                relativePoss.Add(single + unit.Size.Positions[i] - unit.Size.Positions[ii]);
            }

            bool areAll = true;
            foreach (var pos in relativePoss)
            {
                if (!BoardManager.Instance.IsInBounds(pos)) { areAll = false; }
            }

            
            //If all of these coordinates are within a border, return  these positions
            if (areAll)
            {
                return relativePoss; 
            }
        }
        return null;
    }
   
    //Checks if any of the cells are occupied
    public bool AreCellsOccupied(List<Vector2Int> poss)
    {
        foreach(Vector2Int p in poss)
        {
            if (IsInBounds(p))
            {
                if (Board[p.x].Cells[p.y].CurUnit != null) { return false; }
            }     
        }

        return true;
    }
    
    //Converts Vector3 position to a position on the board
    public Vector2Int WorldToBoardPosition(Vector3 pos)
    {
        for (int xi = 0; xi < Board.Count; xi++)
        {
            for (int yi = 0; yi < Board[xi].Cells.Count; yi++)
            {
                if (Board[xi].Cells[yi].Position == pos)
                {                    return new Vector2Int(xi, yi); 
                }
            }
        }

        float fx = (pos.x - CellsObj.transform.position.x - InBetweenSpace) / (InBetweenSpace + CellSize); int x = (int) fx;
        float fy = (CellsObj.transform.position.z - pos.z - InBetweenSpace) / (InBetweenSpace + CellSize); int y = (int) fy;

        Debug.Log(new Vector2Int(x, y));
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

        unit.gameObject.transform.position = BoardToWorldPosition(s).Value + unit.ModelOffset;
        
        yield return new WaitForSeconds(0.1f);
    }

    //Returns true if the position is within bounds of the board
    public bool IsInBounds(Vector2Int v)
    {
        if (v.x < 0 || v.x >= Board.Count) { return false; }
        if (v.y < 0 || v.y >= Board[v.x].Cells.Count) { return false; }

        return true;
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
    public List<Unit> Get_AllUnitsWithAbilities(List<Unit.Ability> abilities)
    {
        List<Unit> units = new List<Unit>();

        foreach (var i in Board)
        {
            foreach (var c in i.Cells)
            {
                if (c == null || c.CurUnit == null) continue;

                if (c.CurUnit.CurAbilities.Intersect<Unit.Ability>(abilities).Any())
                {
                    units.Add(c.CurUnit);
                }
            }
        }

        return units;
    }

    //Returns a list of units with specified keyword
    public List<Unit> Get_AllUnitsWithKeywords(List<Unit.Keyword> keywords)
    {
        List<Unit> units = new List<Unit>();

        foreach (var i in Board)
        {
            foreach (var c in i.Cells)
            {
                if (c == null || c.CurUnit == null) continue;

                if (c.CurUnit.CurKeywords.Intersect<Unit.Keyword>(keywords).Any())
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
    private void Update()
    {
        if (Input.GetKeyDown("p")) { Print(); }

        if (Input.GetKeyDown("b")) { Build(); }

    }
    
}
