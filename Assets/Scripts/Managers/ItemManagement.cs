using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemManagement : MonoBehaviour
{
    #region Singleton
    public static ItemManagement Instance;
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
    void Awake()
    {
        Singleton();

    }
    #endregion
  
    public List<Item> Items;

    public Item CurItem = new Item();
    public List<Item> PossibleItems = new List<Item>();
    public List<Item> AvailableItems = new List<Item>();

    public bool ShouldDebug;

    [Header("---------")]
    public GameObject CurItem_PosObj;
    public GameObject ItemsSelect_Higher;
    public GameObject ItemsSelect_Lower;

    void Start()
    {
        AvailableItems.AddRange(Items); //This is temporary and gives player access to all possible items

        if (ItemsSelect_Higher == null) { ItemsSelect_Higher = GameObject.Find("ItemsPlace_Higher"); }
        if (ItemsSelect_Lower == null) { ItemsSelect_Lower = GameObject.Find("ItemsPlace_Lower"); }

        ClickManager.Instance.E_Click_item.AddListener(UseCurrentItem);
    }
    void OnDisable()
    {
        ClickManager.Instance.E_Click_item.RemoveListener(UseCurrentItem);
    }

    bool ItemCondition()
    {
        Debug.Log("BattleStatsManager.Instance.PlayerTurnActionCondition() " + BattleStatsManager.Instance.PlayerTurnActionCondition());
        Debug.Log("!Action_DrawPlayerResources.IsDeployingNewResources " + !Action_DrawPlayerResources.IsDeployingNewResources);
        Debug.Log("BattleStatsManager.Instance.CurRound " + BattleStatsManager.Instance.CurRound);

        return BattleStatsManager.Instance.PlayerTurnActionCondition() &&   //Only on player's turn
            !Action_DrawPlayerResources.IsDeployingNewResources &&          //Only when not deploying resources
            BattleStatsManager.Instance.CurRound > 0;                      //Not in the deployment round
        
    }

    public IEnumerator DrawPossibleItems(int n)
    {
        if (ShouldDebug) { Debug.Log("Drawing " + n + " possible items"); }
        CurItem = null;
        PossibleItems.Clear();

        List<Item> pItems = new List<Item>(); pItems.AddRange(AvailableItems);
        int safeguard = 1000;
        while (PossibleItems.Count < n && safeguard > 0)
        {
            safeguard--;
            int r = Random.Range(0, pItems.Count);
            //if (ShouldDebug) Debug.Log(pItems.Count + " " + r);

            yield return new WaitForSeconds(Time.fixedDeltaTime);
            PossibleItems.Add(pItems[r]);
            yield return new WaitForSeconds(Time.fixedDeltaTime);
            pItems.Remove(pItems[r]);
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }    
    }

    public IEnumerator SelectNewItem(Item i)
    {
        CurItem = i;
        PossibleItems.Clear();

        yield return new WaitForSeconds(Time.deltaTime);
    }

    Coroutine cUsingItem = null;
    public void UseCurrentItem(Item i)
    {
        if (ShouldDebug) Debug.Log("Using current item");
        if (cUsingItem == null && CurItem != null && ItemCondition())
        {
            cUsingItem = StartCoroutine(UseItem(CurItem));
        }
        else
        {
            //if (ShouldDebug) Debug.Log("Can't launch the item usage coroutine: " + cUsingItem + " " + CurItem);
        }
    }
    
    //Using an item
    public IEnumerator UseItem(Item i)
    {
        Debug.Log("Tried using an item");
        if (!ItemCondition())
        {
            Debug.Log("Failed Using an item");
            yield return null; 
        }
        else
        {
            CurItem.gameObject.SetActive(false);
            Debug.Log("Using an item");

            switch (i.Id)
            {
                case ItemId.water:
                    ActionParameters w = new ActionParameters(GameManager.ActionType.WaterBucket, null, null, null, null, 0);
                    yield return GameManager.Instance.Action(w);
                    break;
                case ItemId.oil:
                    ActionParameters o = new ActionParameters(GameManager.ActionType.OilBucket, null, null, null, null, 0);
                    yield return GameManager.Instance.Action(o);
                    break;
                default:
                    Debug.LogError("Item ID type " + i.Id + " behaviour not set up");
                    break;
            }

            cUsingItem = null;
        }
    
    }
    void UpdateItemPos()
    {
        if (CurItem != null)
        {
            CurItem.transform.position = CurItem_PosObj.transform.position;
        }
    }
    public void SpendCurItem(Item i)
    {
        if (ShouldDebug) Debug.Log("Spent current item");
        if (CurItem == i)
        {
            CurItem = null;
        }
    }
    private void Update()
    {
        UpdateItemPos();
    }
}
