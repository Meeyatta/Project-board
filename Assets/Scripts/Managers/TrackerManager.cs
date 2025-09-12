using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackerManager : MonoBehaviour
{
    #region Singleton
    public static TrackerManager Instance;
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
    #endregion

    void Awake()
    {
        Singleton();
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
