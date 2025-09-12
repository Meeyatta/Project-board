using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellsHolder : MonoBehaviour
{
    public List<BoardCell> Cells;

    private void Start()
    {
        foreach (Transform t in transform)
        {
            if (t.TryGetComponent<BoardCell>(out BoardCell c))
            {
                Cells.Add(c);
            }
        }
    }
}
