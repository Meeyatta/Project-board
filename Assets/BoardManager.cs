using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/*
 LIST OF USEFUL FUNCTIONS IN THIS SCRIPT:

    Build() - Rebuilds all the cells both in the table and in the scene. Doing so sets all cells as unoccupied, must copy the table and Cells object to keep the changes
    Print() - Prints current board and units on it in the console
    Get_UnitPositions(Unit unit) - takes a Unit and return's it's position on the board
    BoardToWorldPosition(List<Vector2Int> poss) - Returns the Vector3 position of a cell under coordinates
    IEnumerator MoveUnit(Unit unit, List<Vector2Int> newPos) - Moves the "unit" to the newPos (If newPos can be moved to)
 */

public class BoardManager : MonoBehaviour
{
    public Unit TestUnit; 

    public float DefaultY;
    public float CellSize;
    public float InBetweenSpace;
    public int Width;
    public int Height;

    public GameObject CellsObj;
    public GameObject BoardCellObj;
    public List<Column> Board;
    
    public static BoardManager Instance;

    #region Events 
    public UnityEvent<Vector2Int> ClickEvent;
    void OnEnable()
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
            Debug.Log(line);
        }
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

    Vector3 BoardToWorldPosition(List<Vector2Int> poss)
    {
        Vector3 newP = new Vector3();
        foreach (Vector2Int v in poss)
        {
            newP.x += Board[v.x].Cells[v.y].Position.x;
            newP.y += Board[v.x].Cells[v.y].Position.y;
            newP.z += Board[v.x].Cells[v.y].Position.z;
        }

        newP.x /= poss.Count; newP.y /= poss.Count; newP.z /= poss.Count;

        return newP;

    }
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

        foreach (Vector2Int v in newPos)
        {
            Board[v.x].Cells[v.y].CurUnit = unit;
        }
        Debug.Log("First position: " + Board[newPos[0].x].Cells[newPos[0].y].Position);
        Debug.Log("Average position: " + BoardToWorldPosition(newPos));

        unit.gameObject.transform.localPosition = Board[newPos[0].x].Cells[newPos[0].y].Position + unit.ModelOffset;

        yield break;
    }
    private void Update()
    {
        if (Input.GetKeyDown("p")) { Print(); }

        if (Input.GetKeyDown("b")) { Build(); }

        if (Input.GetKeyDown("m")) 
        {
            Debug.Log("MOVED THE UNIT");
            List<Vector2Int> newPoss = new List<Vector2Int>();
            foreach (Vector2Int v2 in Get_UnitPositions(TestUnit)) { newPoss.Add(v2 + new Vector2Int(1, 0)); }
            StartCoroutine( MoveUnit(TestUnit, newPoss) ); 
        }

        foreach (Column c in Board) { foreach (Cell cc in c.Cells) { Debug.DrawRay(cc.Position, Vector3.up, Color.red); } }
    }

}
