using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public List<Column> Board;
    public int Width;
    public int Height;

    private void Awake()
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
    private void Update()
    {
        if (Input.GetKeyDown("p")) { Print(); }
    }
}
