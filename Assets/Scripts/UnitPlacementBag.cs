using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitPlacementBag : MonoBehaviour
{
    public GameObject NewUnitsPosObj;
    public static UnitPlacementBag Instance;
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

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
