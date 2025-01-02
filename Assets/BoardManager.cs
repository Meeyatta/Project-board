using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public Unit TestUnit;
    public List<Column> Board;
    public int Width;
    public int Height;

    public static BoardManager Instance;
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
        Board = new List<Column>();
        for (int x = 0; x < Width; x++)
        {
            Column column = new Column();
            column.Cells = new List<Cell>();

            for (int y = 0; y < Height; y++)
            {
                Cell cell = new Cell();
                cell.Coordinates = new Vector2Int(x, y);
                column.Cells.Add(cell);
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

        yield break;
    }
    private void Update()
    {
        if (Input.GetKeyDown("p")) { Print(); }

        
        if (Input.GetKeyDown("m")) 
        {
            Debug.Log("MOVED THE UNIT");
            List<Vector2Int> newPoss = new List<Vector2Int>();
            foreach (Vector2Int v2 in Get_UnitPositions(TestUnit)) { newPoss.Add(v2 + new Vector2Int(1, 0)); }
            StartCoroutine( MoveUnit(TestUnit, newPoss) ); 
        }
    }
}
