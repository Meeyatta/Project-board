using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoDialogue : MonoBehaviour
{
    public float DelayBeforeIntro;
    public List<DialogueLine> PiecesIntro;
    public List<DialogueLine> TorchBearerIntro;
    public List<DialogueLine> UnitMovementIntro;
    public List<DialogueLine> KnightOfCordIntro;
    public List<DialogueLine> ObjectiveIntro1;
    public List<DialogueLine> ObjectiveIntro2;
    public List<DialogueLine> ObjectiveIntro3;
    public List<DialogueLine> DamageIntro;
    public List<DialogueLine> ControlIntro;

    [Header("---------")]
    public bool MovedTorchBearer;
    public bool PassedATurn;
    public bool MovedToObjective;
    public bool UnitDealtDamage;
    public bool MultipleUnitsWithinObjectives;

    IEnumerator StartIntro() 
    {
        yield return new WaitForSeconds(Time.deltaTime * DelayBeforeIntro); 
    }
    void Start()
    {
        StartCoroutine(StartIntro());
    }
    void Update()
    {
        
    }
}
