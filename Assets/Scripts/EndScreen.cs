using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class EndScreen : MonoBehaviour
{
    public string NormalGameSceneName;
    public TextMeshProUGUI pScore;
    public TextMeshProUGUI eScore;
    public TextMeshProUGUI Round;

    const string win = "w";
    const string loss = "l";
    const string restart = "r";

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
        ScoreManager.Instance.IsEnding = true;
        AssignStats();
        Anim.SetTrigger(win);
    }

    public void Loss()
    {
        ScoreManager.Instance.IsEnding = true;
        AssignStats();
        Anim.SetTrigger(loss);
    }

    public async void Restart()
    {
        //The scene restart will need to be remade into manually changing things, but it will work for now
        var scene = SceneManager.LoadSceneAsync(NormalGameSceneName);
        scene.allowSceneActivation = false;
        do
        {
            await Task.Delay(50);
        }
        while (scene.progress < 0.9f);

        ScoreManager.Instance.IsEnding = false;

        Anim.ResetTrigger(win);
        Anim.ResetTrigger(loss);
        Anim.SetTrigger(restart);

        GameManager.Instance.E_Restart.Invoke();

        await Task.Delay(50);

        scene.allowSceneActivation = true;

    }

}
