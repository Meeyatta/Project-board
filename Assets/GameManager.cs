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

    public static GameManager Instance;


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
        //TODO: Do the conditions for checking or selecting a unit
        //switch ()
        //{ 

        //}
        List<Vector2Int> nCoords = new List<Vector2Int>(); nCoords.Add(coords);
        StartCoroutine( BoardManager.Instance.MoveUnit(BoardManager.Instance.TestUnit, nCoords));

        Debug.Log("Clicked " + nCoords);
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
}
