using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public enum RuleMessageInd 
{ 
    PassBeforeDraw, //If player tries to pass a turn before drawing a new unit
    PassingOnWrongTurn, //Passing the turn when it's opponents turn
};

public class RuleMessageManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI MessageObj;

    #region RuleMessage class
    [System.Serializable]
    public class RuleMessage
    {
        public string Name;
        public RuleMessageInd Index;
        public float Duration;
        public string Content;
    }
    #endregion

    public List<RuleMessage> Messages = new List<RuleMessage>();

    #region Singleton
    public static RuleMessageManager Instance;
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

        if (MessageObj == null)
        {
            MessageObj = transform.Find("Message").GetComponent<TextMeshProUGUI>();
        }
    }
    #endregion

    #region Getting the message and its length from a list
    public class floatNstring
    {
        public string str;
        public float fl;

        public floatNstring(string s, float f)
        {
            this.str = s;
            this.fl = f;
        }
    }

    floatNstring GetContent(RuleMessageInd ind)
    {
        string rS = "RULE BREACH";
        float rF = 0;

        foreach (var v in Messages) 
        {
            if (v.Index == ind)
            {
                rS = v.Content;
                rF = v.Duration;
            }
        }

        return new floatNstring(rS, rF);
    }
    #endregion

    #region Playing the selected message
    Coroutine cPMessage = null;
    public void PlayMessage(RuleMessageInd ind)
    {
        if (cPMessage == null)
        {
            cPMessage = StartCoroutine(PlayingMessage(ind));
        }
        else
        {
            StopCoroutine(cPMessage);
            MessageObj.gameObject.SetActive(false);

            cPMessage = StartCoroutine(PlayingMessage(ind));
        }
    }
    IEnumerator PlayingMessage(RuleMessageInd ind)
    {
        yield return new WaitForSeconds(Time.deltaTime);

        floatNstring fns = GetContent(ind);

        MessageObj.text = fns.str;
        MessageObj.gameObject.SetActive(true);

        yield return new WaitForSeconds(fns.fl);

        MessageObj.gameObject.SetActive(false);
        cPMessage = null;
    }
    #endregion
}
