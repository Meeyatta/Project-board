using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Column 
{
    public List<Cell> Cells;
    public Column()
    {
        Cells = new List<Cell>();
    }
}
