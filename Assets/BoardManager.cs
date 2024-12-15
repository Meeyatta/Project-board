using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BoardManager : MonoBehaviour
{
    [SerializeField] public Column[] Board = new Column[10];
    [HideInInspector] public static BoardManager i;
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
