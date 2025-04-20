using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class EndScreen : MonoBehaviour
{
    public TextMeshProUGUI pScore;
    public TextMeshProUGUI eScore;
    public TextMeshProUGUI Round;

    const string win = "w";
    const string loss = "l";

    Animator Anim;
    #region Singleton
    public static EndScreen Instance;
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
        Anim = GetComponent<Animator>();
    }
    #endregion

    public void PlaySound(SoundName name)
    {
        AudioManager.Instance.Play(name, transform);
    }

    void AssignStats()
    {
        pScore.text = ScoreManager.Instance.Score_Player.ToString();
        eScore.text = ScoreManager.Instance.Score_Enemy.ToString();
        Round.text = ScoreManager.Instance.CurRound.ToString();
    }

    public void Win()
    {
        AssignStats();
        Anim.SetTrigger(win);
    }

    public void Loss()
    {
        AssignStats();
        Anim.SetTrigger(loss);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

}
