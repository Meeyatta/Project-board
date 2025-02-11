using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CreationMenu : MonoBehaviour
{
    public static CreationMenu Instance;
    [System.Serializable]
    public class CreationButton
    {
        public Button ButtonObj;
        public GameObject Prefab;
    }
    public List<CreationButton> Buttons = new List<CreationButton>();
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
    void Start()
    {       
        foreach (var button in Buttons) 
        {
            ActionParameters parameters = new ActionParameters(GameManager.ActionType.PlayerCreate, null, null, null, button.Prefab);
            button.ButtonObj.onClick.AddListener( delegate { GameManager.Instance.ActionWrapper(parameters); }); 
        }
    }

    void Update()
    {
        
    }
}
