using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionManager : MonoBehaviour
{
    public Unit TestUnit;
    public Vector2 TestPos;
    public Coroutine CurrentAction;
    
    [HideInInspector] public static ActionManager i;

    void Singleton()
    {
        if (i != null) { Destroy(null); } else { i = this; }
        DontDestroyOnLoad(this);
    }
    IEnumerator Move(Unit u, Vector2 pos)
    {
        yield return StartCoroutine(global::Move.i.Move_Unit(u, pos));
        Debug.Log("Stopped moving");
    }

    
    private void Awake()
    {
        Singleton();
    }
    void Start()
    {
        StartCoroutine(Move(TestUnit, TestPos));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
