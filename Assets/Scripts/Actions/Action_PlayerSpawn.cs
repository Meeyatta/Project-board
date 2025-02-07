using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_PlayerSpawn : MonoBehaviour
{
    public static Action_PlayerSpawn Instance;
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

    public IEnumerator PlayerSpawn(GameObject Object)
    {
        //Create a unit as an object, it's not on the board yet, so it should be hidden
        Unit unit =
            Instantiate(Object, Vector3.zero, Quaternion.identity).GetComponent<Unit>();

        //Make it so we can receive a list of positions and save it
        bool Is_AwaitingData = true;
        List<Vector2Int> v = new List<Vector2Int>();
        void StartAwaiting_ListOfPositions(List<Vector2Int> v2)
        {
            Is_AwaitingData = false;
            v = v2;
            SelectPosition.Instance.ESendPositionBack.RemoveListener(StartAwaiting_ListOfPositions);
        }

        //Add a listener what executes after players selects a position and returns it
        SelectPosition.Instance.ESendPositionBack.AddListener(StartAwaiting_ListOfPositions);

        //Start the action to select a position
        yield return StartCoroutine(SelectPosition.Instance.Selecting(unit));

        //Waiting until we have the data
        while (Is_AwaitingData) { Debug.Log("Awaiting data"); yield return new WaitForSeconds(0.1f); }

        //Place a unit on said selected position
        foreach (Vector2Int vv in v)
        {
            BoardManager.Instance.Board[vv.x].Cells[vv.y].CurUnit = unit;
        }
        unit.gameObject.transform.localPosition = BoardManager.Instance.Board[v[0].x].Cells[v[0].y].Position + unit.ModelOffset;

    }
}
