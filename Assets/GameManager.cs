using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    
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
    private void OnEnable()
    {
        
    }
    private void Start()
    {
        StartCoroutine(Action(AcionType.Move));
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
