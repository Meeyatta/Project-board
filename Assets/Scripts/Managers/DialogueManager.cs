using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public List<DialogueLine> TestLines;
    public float Speed;

    [Header("------------")]
    public bool SkipDialogue = false;
    [Header("------------")]
    public GameObject DialogueWindow;
    public TextMeshProUGUI Text;
    Coroutine CDialogue;

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
        if (CDialogue == null) 
        {
            DialogueWindow.SetActive(true);
            CDialogue = StartCoroutine(SpeakLines(lines));
        }
    }
    public void HideDialogue()
    {
        CDialogue = null;
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
        yield return new WaitForSeconds(Time.deltaTime);

        foreach (DialogueLine line in lines)
        {
            yield return StartCoroutine(SpeakLine(line));

            #region Await until player skips to the next line
            while (!SkipDialogue)
            {
                yield return new WaitForSeconds(Time.deltaTime);
            }
            SkipDialogue = false;
            #endregion
        }

        HideDialogue();
    }

    public IEnumerator SpeakLine(DialogueLine line)
    {
        string words = line.Text;

        Text.text = "";
        foreach (var i in words) 
        {
            if (SkipDialogue)
            {
                Text.text = words;
                SkipDialogue = false;
                yield break;
            }

            Text.text += i;
            yield return new WaitForSeconds(Time.deltaTime * 15f / Speed);
        }
    }
    
    void Start()
    {
        StartDialogue(TestLines);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
