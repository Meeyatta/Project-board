using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_SelectUnit : MonoBehaviour
{
    public static Action_SelectUnit Instance;
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

    public IEnumerator Select(List<Vector2Int> CellsCoordinates)
    {
        if (CellsCoordinates.Count == 0) Debug.LogError("INVALID ACTION PARAMETERS - SELECT(CellCoordinates)");

        Vector2Int coords = CellsCoordinates[0];
        //Debug.Log("SELECTED ON" + coords);

        
        Unit ogUnit = GameManager.Instance.CurUnitSelected; List<Unit> us = new List<Unit>();

        if (GameManager.Instance.CurUnitSelected != BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit)
        {
            GameManager.Instance.CurUnitSelected = BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit;
            ogUnit = GameManager.Instance.CurUnitSelected;
            us.Add(GameManager.Instance.CurUnitSelected);
            GameManager.Instance.ShowMovementEvent.Invoke(us);
        }


        yield return new WaitForSeconds(0.01f);
        while (GameManager.Instance.CurUnitSelected != null && ogUnit == GameManager.Instance.CurUnitSelected)
        {
            //Debug.Log("IS SELECTING A UNIT");
            yield return new WaitForSeconds(0.001f);
        }
        if (us.Count > 0) { GameManager.Instance.HideMovementEvent.Invoke(us); }
    }
}
