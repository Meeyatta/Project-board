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
