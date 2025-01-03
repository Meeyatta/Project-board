using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public Unit TestUnit; 

    public float DefaultY;
    public float CellSize;
    public float InBetweenSpace;
    public int Width;
    public int Height;

    public List<Column> Board;
    
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
                cell.Position = new Vector3(InBetweenSpace + x * (InBetweenSpace + CellSize), DefaultY, -1 * (InBetweenSpace + y * (InBetweenSpace + CellSize)) );


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
    List<Vector3> Get_AllPositions(List<Vector2Int> l)
    {
        List<Vector3> vv = new List<Vector3>();
        foreach (Vector2Int v in l)
        {
            vv.Add(Board[v.x].Cells[v.y].Position);
        }
        return vv;
    }
    Vector3 Get_AveragePosition(List <Vector3> l)
    {
        float avX = 0; float avY = 0; float avZ = 0;
        foreach (Vector3 v in l) { avX += v.x; avY += v.y; avZ += v.z; }

        return new Vector3(avX / l.Count, avY / l.Count, avZ / l.Count);
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

        unit.gameObject.transform.position = Get_AveragePosition(Get_AllPositions(newPos));

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
