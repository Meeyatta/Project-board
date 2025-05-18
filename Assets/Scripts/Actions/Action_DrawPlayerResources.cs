using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//Combination of drawing 1 of 2 new items and 1 of 3 new units
public static class Action_DrawPlayerResources 
{
    #region Stats for units
    static int PulledUnitsAmount = 3;
    static float SpaceBetweenUnits = 2;

    public static UnityEvent<Unit> E_DeployedNewPlayerUnit = new UnityEvent<Unit>();
    #endregion

    #region Stats for items
    static int PulledItemsAmount = 2;
    static float SpaceBetweenItems = 2;

    public static UnityEvent<Unit> E_SelectedNewItem = new UnityEvent<Unit>();
    #endregion

    static bool ShouldDebug = true;
    const string IsHoveringStr = "isHovering";
    const string IsRaisedStr = "isRaised";

    #region IsFocused() - Checks if player is focused on things in front of camera or is looking somewhere else
    static bool IsFocused()
    {
        if (CameraManager.Instance.CurPos == CameraManager.Instance.InFrontOfBoad) return true;

        return false;
    }
    #endregion

    #region Draw3RandomUnits() - returns a list of 3 random untis we can deploy
    static List<Unit> Pull3RandomUnits()
    {
        List<Unit> shuffled = PlayerManager.Instance.ShuffleList(PlayerManager.Instance.Army_NotPlaced);
        List<Unit> pulledUnits = new List<Unit>();
        for (int i = 0; i < Mathf.Min(PulledUnitsAmount, shuffled.Count); i++)
        {
            shuffled[i].gameObject.SetActive(true);
            pulledUnits.Add(shuffled[i]);
            //Debug.Log("Pulled out a " + shuffled[i]);
        }

        return pulledUnits;
    }
    #endregion

    #region SetPos - Sets the pulled units/items beside the listed point
    static void SetPos(GameObject center, List<Unit> units)
    {
        if (ShouldDebug) Debug.Log("SetPos - units " + units.Count);

        for (int i = 0; i < units.Count; i++)
        {
            float half = (float)((units.Count - 1f) / 2f);
            float offsetMod = (half - i) * -1; //Offset modifier to the current unit

            Vector3 offset = new Vector3(SpaceBetweenUnits, 0, 0) * offsetMod;

            //Debug.Log(i + " " + offset);
            if (units[i] != selectedUnit) units[i].transform.position = center.transform.position + offset;

        }
    }
    static void SetPos(GameObject center, List<Item> items)
    {
        if (ShouldDebug) Debug.Log("SetPos - items " + items.Count);

        for (int i = 0; i < items.Count; i++)
        {
            float half = (float)((items.Count - 1f) / 2f);
            float offsetMod = (half - i) * -1; //Offset modifier to the current unit

            Vector3 offset = new Vector3(SpaceBetweenItems, 0, 0) * offsetMod;

            if (ShouldDebug) Debug.Log(center.transform.position + offset);
            if (items[i] != selectedItem) items[i].gameObject.transform.position = center.transform.position + offset;
            if (ShouldDebug) Debug.Log("item pos " + items[i].gameObject.transform.position);

        }
    }
    #endregion


    //Deploys resources for the new round
    //If we need to deploy more than 1 unit or items, should pass a vector with (unitsAmount, itemsAmount)
    static Unit selectedUnit = null;
    static Item selectedItem = null;
    public static bool IsDeployingNewResources = false;
    public static IEnumerator DeployNewResources(ActionParameters parameters)
    {
        IsDeployingNewResources = true;
        yield return new WaitForSeconds(Time.fixedDeltaTime);

        int unitsAmount = 1; int itemsAmount = 1;
   
        if (parameters.CellsCoordinates != null && parameters.CellsCoordinates.Count > 0) 
            { unitsAmount = parameters.CellsCoordinates[0].x; itemsAmount = parameters.CellsCoordinates[0].y; }
        if (ShouldDebug) Debug.Log("pulling out " + unitsAmount + " units and " + itemsAmount + " items");

        bool canDeployNewUnit = PlayerManager.Instance.Army_NotPlaced.Count != 0 && BattleStatsManager.Instance.CurRound != 1;
        bool canDeployNewItem = BattleStatsManager.Instance.CurRound != 1;

        CameraManager.Instance.SetCam(CameraManager.Instance.InFrontOfBoad);

        #region Pulling and hiding new units
        List<Unit> pulledUnits = new List<Unit>();
        IEnumerator PullNewPossibleUnits()
        {
            yield return new WaitForSeconds(Time.fixedDeltaTime);

            if (canDeployNewUnit)
            {
                CameraManager.Instance.SetCam(CameraManager.Instance.InFrontOfBoad);

                pulledUnits.AddRange(Pull3RandomUnits());
                foreach (var v in pulledUnits) { v.Anim.SetBool(IsHoveringStr, true); } //Making units hover 

                void selectAUnit(Unit u) { selectedUnit = u; }

                ClickManager.Instance.E_Click_unit.RemoveListener(selectAUnit);
                ClickManager.Instance.E_Click_unit.AddListener(selectAUnit);
            }
            else
            {
                pulledUnits.Clear();
                unitsAmount = 0;
            }
        }
        IEnumerator HidesPossibleUnits()
        {
            yield return ItemManagement.Instance.DrawPossibleItems(PulledItemsAmount);

            foreach (var v in pulledUnits) 
            { 
                v.Anim.SetBool(IsHoveringStr, false); //Stopping units hover 
                v.transform.position = Vector3.zero;
            } 
            pulledUnits.Clear();
        }
        #endregion

        #region Pulling and hiding new items
        List<Item> pulledItemsCopy = new List<Item>();
        IEnumerator PullNewPossibleItems()
        {
            yield return ItemManagement.Instance.DrawPossibleItems(PulledItemsAmount);

            if (canDeployNewItem)
            {
                CameraManager.Instance.SetCam(CameraManager.Instance.InFrontOfBoad);

                pulledItemsCopy.Clear();
                pulledItemsCopy.AddRange(ItemManagement.Instance.PossibleItems);
                foreach (var v in pulledItemsCopy) { v.Anim.SetBool(IsHoveringStr, true); } //Making items hover 

                void selectAnItem(Item i) { Debug.Log("Selected an item"); selectedItem = i; }

                ClickManager.Instance.E_Click_item.RemoveListener(selectAnItem);
                ClickManager.Instance.E_Click_item.AddListener(selectAnItem);
            }
            else
            {
                pulledItemsCopy.Clear();
                itemsAmount = 0;
            }
        }
        IEnumerator HidesPossibleItems()
        {
            yield return new WaitForSeconds(Time.fixedDeltaTime);

            foreach (var v in pulledItemsCopy)
            {
                v.Anim.SetBool(IsHoveringStr, false); //Stopping units hover 
                v.transform.position = Vector3.zero - Vector3.left * 5;
            }
            pulledUnits.Clear();
        }
        #endregion

        bool placedUnit = !canDeployNewUnit; bool placedItem = !canDeployNewItem;

        yield return PullNewPossibleUnits();
        yield return PullNewPossibleItems();
        while (unitsAmount > 0 || itemsAmount > 0)
        {
            yield return new WaitForSeconds(Time.fixedDeltaTime / 1000);
            if (ShouldDebug) Debug.Log("Continuing " + unitsAmount + " " + itemsAmount);

            #region If player focuses on new resources
            if (IsFocused())
            {
                SetPos(UnitPlacementBag.Instance.UnitsPos_InFront, pulledUnits);
                SetPos(ItemManagement.Instance.ItemsSelect_Higher, pulledItemsCopy);

                #region Raising a unit we hover over
                if (ClickManager.Instance.CurrentPointedAtUnit != null)
                {
                    foreach (var v in pulledUnits)
                    {
                        if (v == ClickManager.Instance.CurrentPointedAtUnit)
                        {
                            v.Anim.SetBool(IsRaisedStr, true);
                        }
                        else
                        {
                            v.Anim.SetBool(IsRaisedStr, false);
                        }
                    }
                }
                #endregion

                #region Raising an item we hover over
                if (ClickManager.Instance.CurrentPointedAtItem != null)
                {
                    foreach (var v in pulledItemsCopy)
                    {
                        if (v == ClickManager.Instance.CurrentPointedAtItem)
                        {
                            v.Anim.SetBool(IsRaisedStr, true);
                        }
                        else
                        {
                            v.Anim.SetBool(IsRaisedStr, false);
                        }
                    }
                }
                #endregion
            }
            #endregion
            #region If looking somewhere else
            else
            {
                SetPos(UnitPlacementBag.Instance.UnitsPos_Below, pulledUnits);
                SetPos(ItemManagement.Instance.ItemsSelect_Lower, pulledItemsCopy);
            }
            #endregion

            #region Placing the selected unit
            if (selectedUnit != null && unitsAmount > 0)
            {
                selectedUnit.Anim.SetBool(IsHoveringStr, false);
                selectedUnit.Anim.SetBool(IsRaisedStr, false);
                PlayerManager.Instance.RecordPlacedUnit(selectedUnit);

                GameManager.Instance.E_ShowDeployment.Invoke(new List<Unit> { selectedUnit });
                ActionParameters createPar = new ActionParameters(
                            GameManager.ActionType.PlayerCreate, new List<Unit> { selectedUnit }, null, null, null, 0);
                yield return Action_PlayerCreate.PlayerCreate(createPar);

                if (ShouldDebug) Debug.Log("left with " + unitsAmount + " units and " + itemsAmount + " items");
                if (pulledUnits.Contains(selectedUnit)) pulledUnits.Remove(selectedUnit);
                selectedUnit = null;
                unitsAmount--;
                yield return HidesPossibleUnits();
                yield return new WaitForSeconds(Time.fixedDeltaTime);
                GameManager.Instance.E_HideDeployment.Invoke();

                if (unitsAmount > 0) yield return PullNewPossibleUnits();
                yield return new WaitForSeconds(Time.fixedDeltaTime);
            }
            #endregion

            #region Selecting an item
            if (selectedItem != null && itemsAmount > 0)
            {
                yield return ItemManagement.Instance.SelectNewItem(selectedItem);

                if (ShouldDebug) Debug.Log("left with " + unitsAmount + " units and " + itemsAmount + " items");
                itemsAmount--;
                yield return HidesPossibleItems();

                if (itemsAmount > 0) PullNewPossibleItems();
                yield return new WaitForSeconds(Time.fixedDeltaTime);
            }
                #endregion
        }

        if (ShouldDebug) Debug.Log("Ended DeployNewResources");
        IsDeployingNewResources = false;
        yield return new WaitForSeconds(Time.fixedDeltaTime);
        GameManager.Instance.RemoveAction(parameters);
    }

}
