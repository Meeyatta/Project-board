using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class Action_SelectPosition : MonoBehaviour
{
    public Unit CurUnit;
    public UnityEvent<List<Vector2Int>> ESendPositionBack;
    public bool IsAwaitingAClickBack = true;
    public static Action_SelectPosition Instance;
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
    void StopWaiting(List<Vector2Int> v)
    {
        List<Vector2Int> nv = new List<Vector2Int>();
        IsAwaitingAClickBack = false;

        if (v.Count == 1 && CurUnit != null) 
        {
            for (int i = 0; i < CurUnit.Size.Positions.Count; i++)
            {
                nv.Add(v[i] + CurUnit.Size.Positions[i]);
                Debug.Log("Will send " + (v[i] + CurUnit.Size.Positions[i]) + " back to GameManager");
            }
        }

        ESendPositionBack.Invoke(nv);
    }
    public IEnumerator SelectPosition(Unit unit)
    {
        GameManager.Instance.ClickBackEvent.AddListener(StopWaiting);
        CurUnit = unit;

        IsAwaitingAClickBack = true;
        while (IsAwaitingAClickBack) { yield return new WaitForSeconds(0.01f); }

        GameManager.Instance.ClickBackEvent.RemoveListener(StopWaiting);

        yield return new WaitForSeconds(0.001f);
    }

}
