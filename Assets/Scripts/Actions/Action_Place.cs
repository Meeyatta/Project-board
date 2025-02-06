using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class Action_Place : MonoBehaviour
{
    public static Action_Place Instance;
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
    }

    public IEnumerator Place(GameObject Object,List<Vector2Int> CellsCoordinates)
    {
        if (!BoardManager.Instance.BoardToWorldPosition(CellsCoordinates).HasValue) 
        {
            Debug.LogError("INVALID PLACING POSITION FOR " + Object.name); yield return null; 
        }

        Object.transform.position = BoardManager.Instance.BoardToWorldPosition(CellsCoordinates).Value;
        Object.transform.rotation = Quaternion.identity;
        Unit u = Object.GetComponent<Unit>();

        bool canPlace = true; 
        foreach (Vector2Int v in CellsCoordinates) 
        { 
            if (BoardManager.Instance.Board[v.x].Cells[v.y].CurUnit != null) { canPlace = false; break; } 
        }

        if (canPlace) { foreach (Vector2Int v in CellsCoordinates) { BoardManager.Instance.Board[v.x].Cells[v.y].CurUnit = u; } }
        else { Debug.LogError("INVALID PLACING POSITION FOR " + Object.name); }

        yield return new WaitForSeconds(0.001f);
    }

}
