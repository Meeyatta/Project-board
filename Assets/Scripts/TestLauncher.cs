using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLauncher : MonoBehaviour
{
    public BattleParameters Battle_Small;
    public BattleParameters Battle_Medium;
    public BattleParameters Battle_Large;


    void Start()
    {

        //StartCoroutine(BattleManager.Instance.StartBattle(Battle_Small));
        StartCoroutine(BattleManager.Instance.StartBattle(Battle_Medium));
        //StartCoroutine(BattleManager.Instance.StartBattle(Battle_Large));
    }

    void Update()
    {
        
    }
}
