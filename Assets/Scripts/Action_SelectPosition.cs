using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class Action_SelectPosition : MonoBehaviour
{
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

    public IEnumerator SelectPosition(GameObject Object)
    {
        //TODO: Both instantiate and GetComponent are expensive on performance, change it later
        

        yield return new WaitForSeconds(0.001f);
    }

}
