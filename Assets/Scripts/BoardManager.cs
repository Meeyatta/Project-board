using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

/*
  Functionality:

    Build() - Rebuilds all the cells both in the table and in the scene. Doing so sets all cells as unoccupied, must copy the table and Cells object to keep the changes
    Print() - Prints current board and units on it in the console
    Get_UnitPositions(Unit unit) - takes a Unit and return's it's position on the board
    BoardToWorldPosition(List<Vector2Int> poss) - Returns the Vector3 position of a cell under coordinates
    IEnumerator MoveUnit(Unit unit, List<Vector2Int> newPos) - Moves the "unit" to the newPos (If newPos can be moved to)
    Get_AllUnitsOnBoard() - Returns a list of all units on board cells
    CursorToCellPosition() - Returns the cell under the player's cursor
 */

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
            if (areAll) { return relativePoss; }
        }
        return null;
    }

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
                res = CellClosestToCursor(hit.point);
            }
        }
        else
        {
            res = lastPres;
        }

        lastPres = res;
        return res;
    }
    Vector2Int CellClosestToCursor(Vector3 hitPosition)
    {
        Vector2Int res = new Vector2Int(0, 0);
        float minDist = Mathf.Infinity;
        for (int x = 0; x < Board.Count; x++) { for (int y = 0; y < Board[x].Cells.Count; y++) 
            { 
                if (Board[x].Cells[y].CurUnit != null) { continue; }
                if (Vector3.Distance(Board[x].Cells[y].Position, hitPosition) < minDist) { minDist = Vector3.Distance(Board[x].Cells[y].Position, hitPosition); res = new Vector2Int(x, y); }
        } }

        return res;
    }
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
    public Vector2Int WorldToBoardPosition(Vector3 pos)
    {

        float fx = (pos.x - CellsObj.transform.position.x - InBetweenSpace) / (InBetweenSpace + CellSize); int x = (int) fx;
        float fy = (CellsObj.transform.position.z - pos.z - InBetweenSpace) / (InBetweenSpace + CellSize); int y = (int) fy;

        return new Vector2Int(x, y);
    }
    public Vector3? BoardToWorldPosition(List<Vector2Int> poss)
    {

        Vector3 newP = Vector3.zero;
        if (poss.Count <= 0) { return newP; }
        foreach (Vector2Int v in poss)
        {
            if (!IsInBounds(v)) return null;

            //Debug.Log("newP " + v);

            newP.x += Board[v.x].Cells[v.y].Position.x;
            newP.y += Board[v.x].Cells[v.y].Position.y;
            newP.z += Board[v.x].Cells[v.y].Position.z;
        }

        newP.x /= poss.Count; newP.y /= poss.Count; newP.z /= poss.Count;

        return newP;

    }

    //Places unit onto cells under the positions (So doesn't change the position the unit is currently occupying)
    public IEnumerator PlaceUnit(Unit unit, List<Vector2Int> newPos)
    {
        foreach (Vector2Int v in newPos)
        {
            Board[v.x].Cells[v.y].CurUnit = unit;
        }
        //Debug.Log("First position: " + Board[newPos[0].x].Cells[newPos[0].y].Position);
        //Debug.Log("Average position: " + BoardToWorldPosition(newPos));

        unit.gameObject.transform.localPosition = Board[newPos[0].x].Cells[newPos[0].y].Position + unit.ModelOffset;

        yield break;
    }

    //Moves unit from it's original position to the new one
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
   
    private void Update()
    {

        if (Input.GetKeyDown("p")) { Print(); }

        if (Input.GetKeyDown("b")) { Build(); }


        foreach (Column c in Board) { foreach (Cell cc in c.Cells) { Debug.DrawRay(cc.Position, Vector3.up, Color.red); } }
    }
    public bool IsInBounds(Vector2Int v)
    {      
        if (v.x < 0 || v.x >= Board.Count) { return false; }
        if (v.y < 0 || v.y >= Board[v.x].Cells.Count) { return false; }

        return true;
    }
}
