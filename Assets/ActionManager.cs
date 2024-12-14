using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionManager : MonoBehaviour
{
    [HideInInspector] public ActionManager i;

    public void Move()
    {

    }

    void Singleton()
    {
        if (i != null) { Destroy(null); } else { i = this; }
        DontDestroyOnLoad(this);
    }
    private void Awake()
    {
        Singleton();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
