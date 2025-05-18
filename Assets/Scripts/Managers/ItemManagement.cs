using System.Collections;
using System.Collections.Generic;
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

    public Item CurItem;
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
    }

    public IEnumerator DrawPossibleItems(int n)
    {
        PossibleItems.Clear();

        List<Item> pItems = new List<Item>(); pItems.AddRange(AvailableItems);
        int safeguard = 1000;
        while (PossibleItems.Count < n && safeguard > 0)
        {
            safeguard--;
            int r = Random.Range(0, pItems.Count);
            if (ShouldDebug) Debug.Log(pItems.Count + " " + r);

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

    private void Update()
    {
        if (CurItem != null)
        {
            CurItem.transform.position = CurItem_PosObj.transform.position;
        }
    }
}
