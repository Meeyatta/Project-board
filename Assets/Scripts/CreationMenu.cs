using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CreationMenu : MonoBehaviour
{
    GameObject Canvas;
    public static CreationMenu Instance;
    [System.Serializable]
    public class CreationButton
    {
        public string Name;
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
        Canvas = transform.Find("Canvas").gameObject;
        foreach (var button in Buttons) 
        {
            ActionParameters parameters = new ActionParameters(GameManager.ActionType.PlayerCreate, null, null, null, button.Prefab);

            button.ButtonObj.onClick.AddListener(delegate { Create(parameters); });
        }

        TurnOff();
    }
    void Create( ActionParameters parameters)
    {
        GameManager.Instance.ActionWrapper(parameters);
        TurnOff();
    }
    public void TurnOn()
    {
        Canvas.SetActive(true);
    }
    public void TurnOff()
    {
        Canvas.SetActive(false);
    }
    void Update()
    {
        if (Input.GetKeyDown("c"))
        {
            if (Canvas.activeSelf) { TurnOff(); } else { TurnOn(); }   
        }
    }
}
