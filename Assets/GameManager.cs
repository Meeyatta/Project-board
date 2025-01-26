using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    #region Events 
    public UnityEvent<Vector2Int> ClickEvent;
    void OnEnable()
    {
        ClickEvent.AddListener(CellClick);
    }
    void OnDisable()
    {
        ClickEvent.RemoveListener(CellClick);
    }
    #endregion Events

    public enum AcionType { Move };
    public Unit CurUnitSelected; //What unit is currently selected, if no unit - should be null
    public static GameManager Instance;
    public Coroutine ClickCoroutine;

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
    
    private void Start()
    {
        StartCoroutine(Action(AcionType.Move));
    }
    void CellClick(Vector2Int coords)
    {
        if (ClickCoroutine == null)
        {
            ClickCoroutine = StartCoroutine(CellClickCoroutine(coords));
        }
        else
        {
            Debug.Log("Occupied - " + ClickCoroutine);
        }
    }
    IEnumerator CellClickCoroutine(Vector2Int coords)
    {
        yield return new WaitForSeconds(0.001f); //For some reason 

        if (CurUnitSelected != null && BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit == null) //If have a unit and cell is unoccupied - a)move it to the cell, otherwise - b)select a unit on that cell
        { //a)
            List<Vector2Int> nCoords = new List<Vector2Int>(); nCoords.Add(coords);
            yield return StartCoroutine(BoardManager.Instance.MoveUnit(CurUnitSelected, nCoords));
            CurUnitSelected = null;

            Debug.Log("MOVED TO " + nCoords);

        }
        else
        { //b)
            if (BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit != null) //Check whenever there is a unit on a clicked cell
            {
                CurUnitSelected = BoardManager.Instance.Board[coords.x].Cells[coords.y].CurUnit;
                Debug.Log("SELECTED ON" + coords);

            }
            else
            {
                Debug.Log("HAVE NOTHING SELECTED, " + coords + " HAS NO UNITS ");
                //No unit on that cell, do nothing


            }
        }

        ClickCoroutine = null;
        yield return null;
    }
    public IEnumerator Action(AcionType type)
    {
        switch (type)
        {
            case AcionType.Move:
                
                break;
            default:
                Debug.LogError("Action not written");
                break;
        }
        yield return null;
    }
    private void Update()
    {

    }
}
