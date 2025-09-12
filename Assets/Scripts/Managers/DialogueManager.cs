using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public float Speed;
    public float DelayBeforeAutoSkip;

    [Header("------------")]
    public bool SkipDialogue = false;
    [Header("------------")]
    public GameObject DialogueWindow;
    public TextMeshProUGUI Text;
    Coroutine cDialogue;
    Coroutine cLine;

    #region Singleton
    public static DialogueManager Instance;
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
    void OnEnable()
    {
        Singleton();
    }
    #endregion
    public void StartDialogue(List<DialogueLine> lines)
    {   
        if (cDialogue != null) StopCoroutine(cDialogue);

        DialogueWindow.SetActive(true);
        cDialogue = StartCoroutine(SpeakLines(lines));      
    }
    public void HideDialogue()
    {
        cDialogue = null;
        DialogueWindow.SetActive(false);
    }
    public void Skip(InputAction.CallbackContext context)
    {
        if (context.performed) 
        {
            SkipDialogue = true;
        }
    }

    public IEnumerator SpeakLines(List<DialogueLine> lines)
    {
        DialogueWindow.SetActive(true);

        foreach (DialogueLine line in lines)
        {

            if (cLine != null) { StopCoroutine(cLine); }
            yield return cLine = StartCoroutine(SpeakLine(line));
            
            #region Await until player skips to the next line
            float skipMoment = Time.time + DelayBeforeAutoSkip;
            while (!SkipDialogue && skipMoment > Time.time)
            {
                if (cLine != null) skipMoment = Time.time + DelayBeforeAutoSkip;
                yield return new WaitForSeconds(Time.fixedDeltaTime);
            }
            SkipDialogue = false;
            #endregion
        }

        Debug.Log("Stopped speaking lines");
        HideDialogue();
    }

    public IEnumerator SpeakLine(DialogueLine line)
    {
        string words = line.Text;

        Text.text = "";
        foreach (var i in words) 
        {
            if (SkipDialogue && cLine != null)
            {
                Text.text = words;
                SkipDialogue = false;
                yield break;
            }

            Text.text += i;
            yield return new WaitForSeconds(Time.fixedDeltaTime * 100 / Speed);
        }


        cLine = null;
    }
    
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
