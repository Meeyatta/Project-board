using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PassTurnButton : MonoBehaviour
{
    public GameObject Button;
    public bool CanPass = true;
    void Start()
    {
        ScoreManager.Instance.TurnEvent.AddListener(EnablePass);
    }
    void OnDisable()
    {
        ScoreManager.Instance.TurnEvent.RemoveListener(EnablePass);
    }
    void EnablePass(ScoreManager.Side side)
    {
        if (side == ScoreManager.Side.Player) 
        {
            CanPass = true;
        }
    }
    public void Pass()
    {
        if (ScoreManager.Instance.CurTurn == ScoreManager.Side.Player && CanPass)
        {
            CanPass = false;
            ActionParameters parameters = new ActionParameters(GameManager.ActionType.NextTurn, null, null, null, null, 0);
            StartCoroutine(GameManager.Instance.Action(parameters));
        }
    }
    void FixedUpdate()
    {
        Button.transform.LookAt(Camera.main.transform.position);
    }
}
