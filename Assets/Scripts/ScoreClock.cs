using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using Unity.VisualScripting;

public class ScoreClock : MonoBehaviour
{
    public Color CalmColor;
    public Color AttentionColor;
    public TextMeshProUGUI PlayerPointsText;
    public TextMeshProUGUI SymbolText; //This is the symbol between player and enemy points to show the relative number
    public TextMeshProUGUI EnemyPointsText;

    public UnityEvent<Side> e_PointScored;

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
        e_PointScored.Invoke(Side.Player);
        Anim.SetTrigger(Shrugstr);
    }
    public void Shrug_Visuals(Side s)
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
        PlayerPointsText.text = BattleStatsManager.Instance.Score_Player.ToString();
        EnemyPointsText.text = BattleStatsManager.Instance.Score_Enemy.ToString();

        #region Changing the color if player is close to losing
        if (BattleStatsManager.Instance.Score_Enemy - BattleStatsManager.Instance.Score_Player  == BattleStatsManager.Instance.NScoreDiff - 1 )
        {
            EnemyPointsText.color = AttentionColor;
            SymbolText.color = AttentionColor;
        }
        else 
        { 
            EnemyPointsText.color = CalmColor;
            SymbolText.color = CalmColor;
        }
        #endregion

        #region Displaying the differenc symbol
        if (BattleStatsManager.Instance.Score_Player == BattleStatsManager.Instance.Score_Enemy) 
        {
            SymbolText.text = "="; 
        }
        else
        {
            string res = (BattleStatsManager.Instance.Score_Player - BattleStatsManager.Instance.Score_Enemy).ToString(); 
            if (BattleStatsManager.Instance.Score_Player > BattleStatsManager.Instance.Score_Enemy) { res = "+" + res; }
            SymbolText.text = res;
        }
        #endregion

    }
    
    void FixedUpdate()
    {
        UpdateText();
    }
}
