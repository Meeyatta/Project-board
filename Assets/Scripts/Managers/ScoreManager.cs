using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ScoreManager : MonoBehaviour
{
    //Event for the conclusion of the battle 
    public UnityEvent<Side> WinEvent;
    public static ScoreManager Instance;
    public enum Side { Player, Enemy };
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

    public int NScoreDiff;
    public int Score_Player;
    public int Score_Enemy;

    void Update()
    {
        if (Score_Player >= Score_Enemy + NScoreDiff) { WinEvent.Invoke(Side.Player); Debug.Log("Player has won"); }

        if (Score_Enemy >= Score_Player + NScoreDiff) { WinEvent.Invoke(Side.Enemy); Debug.Log("Enemy has won"); }
    }
}
