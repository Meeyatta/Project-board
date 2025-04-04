using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreClock : MonoBehaviour
{

    public TextMeshProUGUI PlayerPointsText;
    public TextMeshProUGUI SymbolText; //This is the symbol between player and enemy points to show the relative number
    public TextMeshProUGUI EnemyPointsText;

    [HideInInspector]
    const string Shrugstr = "shrug";
    public Animator Anim;

    #region Singleton
    public static ScoreClock Instance;
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
    #endregion
    void Start()
    {
        Anim = GetComponent<Animator>();
    }


    #region Visual effects for when the turnButton is activated, for in-script use and event
    public void Shrug_Visuals()
    {
        Anim.SetTrigger(Shrugstr);
    }
    public void Shrug_Visuals(ScoreManager.Side s)
    {
        Shrug_Visuals();
    }
    #endregion

    public void PlaySound(SoundName n)
    {
        AudioManager.Instance.Play(n, transform);
    }
    public void UpdateText()
    {
        PlayerPointsText.text = ScoreManager.Instance.Score_Player.ToString();
        EnemyPointsText.text = ScoreManager.Instance.Score_Enemy.ToString();


        #region Displaying current points

        if (ScoreManager.Instance.Score_Player == ScoreManager.Instance.Score_Enemy) 
        {
            SymbolText.text = "="; 
        }
        else
        {
            string res = (ScoreManager.Instance.Score_Player - ScoreManager.Instance.Score_Enemy).ToString(); 
            if (ScoreManager.Instance.Score_Player > ScoreManager.Instance.Score_Enemy) { res = "+" + res; }
            SymbolText.text = res;
        }
        #endregion
    }
    
    void FixedUpdate()
    {
        UpdateText();
    }
}
