using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : Action
{
    [HideInInspector] public static Move i;
    void Singleton()
    {
        if (i != null) { Destroy(null); } else { i = this; }
        DontDestroyOnLoad(this);
    }
    private void Awake()
    {
        Singleton();


    }
    public IEnumerator Move_Unit(Unit u, Vector2 pos)
    {
        yield return new WaitForSeconds(3);
        yield return null;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
