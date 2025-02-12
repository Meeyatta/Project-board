using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_PlayerCreate : MonoBehaviour
{
    public bool IsWaitingForData;
    public static Action_PlayerCreate Instance;
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

    public IEnumerator PlayerCreate(GameObject Object)
    {
        //Create a unit as an object, it's not on the board yet, so it should be hidden
        Unit unit =
            Instantiate(Object, Vector3.zero, Quaternion.identity).GetComponent<Unit>();
        List<Unit> unitList = new List<Unit>(); unitList.Add(unit);


        //Add a listener what executes after players selects a position and returns it
        List<Vector2Int> positions = new List<Vector2Int>();

        void StartAwaiting_ListOfPositions(List<Vector2Int> v2)
        {
            IsWaitingForData = false;
            positions = v2;
            SelectPosition.Instance.ESendPositionBack.RemoveListener(StartAwaiting_ListOfPositions);
        }
        SelectPosition.Instance.ESendPositionBack.AddListener(StartAwaiting_ListOfPositions);
        IsWaitingForData = true;

        //Start the action to select a position
        GameManager.Instance.ShowPlacementEvent.Invoke(unitList);
        GameManager.Instance.I_PositionSelect = SelectPosition.Instance.Selecting(unit);
        yield return StartCoroutine(GameManager.Instance.I_PositionSelect);
        GameManager.Instance.I_PositionSelect = null;

        //Waiting until we have the data
        while (IsWaitingForData) { yield return new WaitForSeconds(0.1f); }

        //Check if can place a unit there

        bool ViablePos = true;
        foreach (var v in positions) 
        {
            if (!BoardManager.Instance.IsInBounds(v)) { ViablePos = false; break; }

            if (BoardManager.Instance.Board[v.x].Cells[v.y].CurUnit != null) { ViablePos = false; break; }
        }

        //Place a unit on said selected positions if they are viable
        GameManager.Instance.HidePlacementEvent.Invoke(unitList);
        if (ViablePos) 
        {
            yield return StartCoroutine(BoardManager.Instance.PlaceUnit(unit, positions));
        }
        else
        {
            Debug.Log("Non viable position");
        }

        yield return new WaitForSeconds(0.001f);
    }

}
