using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PassTurnButton : MonoBehaviour
{
    public GameObject Button;
    public bool CanPass = true;

    string Shrugstr = "Shrug";
    Animator anim;
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    void Start()
    {
        ScoreManager.Instance.TurnEvent.AddListener(EnablePass);
    }
    void OnDisable()
    {
        ScoreManager.Instance.TurnEvent.RemoveListener(EnablePass);
    }
    public void PlaySound()
    {
        AudioManager.Instance.Play(SoundName.ClockPling, transform);
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
            if (ScoreManager.Instance.CurRound != 0) CanPass = false;
            anim.SetTrigger(Shrugstr);
            ActionParameters parameters = new ActionParameters(GameManager.ActionType.NextTurn, null, null, null, null, 0);
            StartCoroutine(GameManager.Instance.Action(parameters));
        }
    }
    void FixedUpdate()
    {
        Button.transform.LookAt(Camera.main.transform.position);
    }
}
