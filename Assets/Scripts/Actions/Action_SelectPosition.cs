using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class Action_SelectPosition : MonoBehaviour
{
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
    void StopWaiting(Vector2Int v)
    {
        IsAwaitingAClickBack = false;
    }
    public IEnumerator SelectPosition(GameObject Object)
    {
        GameManager.Instance.ClickBackEvent.AddListener(StopWaiting);

        IsAwaitingAClickBack = true;
        while (IsAwaitingAClickBack) { yield return new WaitForSeconds(0.01f); }

        GameManager.Instance.ClickBackEvent.RemoveListener(StopWaiting);
        yield return new WaitForSeconds(0.001f);
    }

}
