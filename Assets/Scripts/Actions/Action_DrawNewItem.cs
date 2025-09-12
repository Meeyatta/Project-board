using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is deprecated, functions as a part of Action_DrawPlayerResources
public static class Action_DrawNewItem
{
    static int AmountOfPossibleItems = 2;
    static bool ShouldDebug = true;
    public static bool IsDrawingNewItem = false;

    const string IsHoveringStr = "isHovering";
    const string IsRaisedStr = "isRaised";

    public static IEnumerator DrawNewItem(ActionParameters parameters)
    {
        yield return DrawNewItem();

        yield return new WaitForSeconds(0.1f * Time.fixedDeltaTime);
        GameManager.Instance.RemoveAction(parameters);
    }

    public static IEnumerator DrawNewItem()
    {
        IsDrawingNewItem = true;
        yield return new WaitForSeconds(Time.fixedDeltaTime);

        if (ShouldDebug) Debug.Log("Initiated DrawPossibleItems of " + AmountOfPossibleItems);
        
        yield return ItemManagement.Instance.DrawPossibleItems(AmountOfPossibleItems);
        yield return new WaitForSeconds(Time.fixedDeltaTime);

        if (ShouldDebug) 
            Debug.Log("Finished DrawPossibleItems, got: " + ItemManagement.Instance.PossibleItems[0] + " " + ItemManagement.Instance.PossibleItems[1]);

        //Camera is drawn in front of the board
        CameraManager.Instance.SetCam(CameraManager.Instance.InFrontOfBoad);

        #region Hovering the possible items until player selects one
        Item selectedItem = null; List<Item> pulledItemsCopy = new List<Item>(); pulledItemsCopy.AddRange(ItemManagement.Instance.PossibleItems);

        foreach (var v in pulledItemsCopy) { v.Anim.SetBool(IsHoveringStr, true); } //Making items hover 

        #region Adding an event for selecting an item player clicked on
        void stopAwaitingSelection(Item i) { Debug.Log("Selected an item"); ; selectedItem = i; }

        if (ShouldDebug) Debug.Log("AddListener ");
        ClickManager.Instance.E_Click_item.RemoveListener(stopAwaitingSelection);
        ClickManager.Instance.E_Click_item.AddListener(stopAwaitingSelection);
        #endregion

        while (selectedItem == null)
        {
            yield return new WaitForSeconds(Time.fixedDeltaTime / 1000);
        }

        IsDrawingNewItem = false;
        #endregion
    }
}
