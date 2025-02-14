using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SelectPosition : MonoBehaviour
{
    public Unit CurUnit;
    public UnityEvent<List<Vector2Int>> ESendPositionBack;
    public bool IsAwaitingAClickBack = true;
    public static SelectPosition Instance;
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

    public IEnumerator Selecting(Unit unit)
    {
        void StopWaiting(Vector2Int v)
        {
            List<Vector2Int> nv = new List<Vector2Int>();
            IsAwaitingAClickBack = false;

            if (CurUnit != null)
            {
                for (int i = 0; i < CurUnit.Size.Positions.Count; i++)
                {
                    nv.Add(v + CurUnit.Size.Positions[i]);
                    //Debug.Log("Will send " + (v[i] + CurUnit.Size.Positions[i]) + " back to GameManager");
                }
            }

            ESendPositionBack.Invoke(nv);
        }
        GameManager.Instance.ClickBackEvent.AddListener(StopWaiting);
        CurUnit = unit;

        IsAwaitingAClickBack = true;
        while (IsAwaitingAClickBack) { yield return new WaitForSeconds(0.01f); }

        GameManager.Instance.ClickBackEvent.RemoveListener(StopWaiting);

        yield return new WaitForSeconds(0.001f);
    }

}
