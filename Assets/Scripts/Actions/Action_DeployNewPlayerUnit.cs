using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public static class Action_DeployNewPlayerUnit 
{
    static int PulledNumber = 3;

    const string IsHoveringStr = "IsHovering";
    const string IsRaisedStr = "isRaised";

    #region Finds a unit from a list using its transform
    static Unit GetUnitWithTransform(List<Unit> units, Transform t)
    {
        foreach (var v in units)
        {
            if (t.IsChildOf(v.transform)) { return v; }
            if (t.parent != null && t.parent.transform == t) { return v; }
            if (v.transform == t) { return v; }
        }
        return null;
    }
    #endregion


    #region Sets the pulled units beside the listed point
    static void SetPos(GameObject center, List<Unit> units)
    {
        for (int i = 0; i < units.Count; i++)
        {
            float half = (float)((units.Count - 1f) / 2f);
            float offsetMod = (half - i) * -1; //Offset modifier to the current unit

            Vector3 offset = new Vector3(UnitPlacementBag.Instance.SpaceBetweenUnits, 0, 0) * offsetMod;

            //Debug.Log(i + " " + offset);
            units[i].transform.position = center.transform.position + offset;

        }
    }
    #endregion

    #region Checks if player is focused on units in front of camera or is looking somewhere else
    static bool IsFocused()
    {
        if (CameraManager.Instance.CurPos == CameraManager.Instance.InFrontOfBoad) return true;

        return false;
    }
    #endregion

    public static IEnumerator DeployNewUnit(ActionParameters parameters)
    {
        yield return new WaitForSeconds(Time.deltaTime);

        /*
         1) Camera is drawn in front of the board
            The bag plays a shuffle animation and a sound
         2) Up to 3 units are pulled out in an arc to the right, then to the left

         3) 3 unit models hover in front of the player up and down
            Player can move camera after that, but cannot interact with other things
            If camera is not looking at the units, they are placed closer to the board, clicking on them gets player back to the unit selection
            Hovering over the model raises the model slightly, shows a prompt (M1 - select, M2 - Check data)

            4Select) The rest of the units are dragged back into the bag with a shuffling sound, player now places a new unit within the deployment zone
            4CheckData) TODO:
         */

        //Camera is drawn in front of the board
        CameraManager.Instance.SetCam(CameraManager.Instance.InFrontOfBoad);

        //The bag plays a shuffle animation and a sound
        UnitPlacementBag.Instance.PullOutUnits();

        //3 unit models hover in front of the player up and down 
        List<Unit> shuffled = ResourceManager.Instance.ShuffleList(ResourceManager.Instance.Army_NotPlaced);
        List<Unit> pulledUnits = new List<Unit>();
        for (int i = 0; i < Mathf.Min(PulledNumber, shuffled.Count); i++)
        {
            pulledUnits.Add(shuffled[i]);
            //Debug.Log("Pulled out a " + shuffled[i]);
        }

        //Save the units we are going to show
        GameObject unitsPoint = UnitPlacementBag.Instance.UnitsPos_InFront;

        #region Showing the selected units
        Unit selectedUnit = new Unit();
        bool IsSelecting = true;
        while (IsSelecting)
        {
            yield return (Time.deltaTime);
            //Debug.Log("Showing units in front of player");

            foreach (var v in pulledUnits) { v.Anim.SetBool(IsHoveringStr, true); } //Making units hover 

            //Checking if player is looking at unit or swapped camera position
            if (IsFocused())
            #region If focused on new units
            {
                SetPos(UnitPlacementBag.Instance.UnitsPos_InFront, pulledUnits);

                #region Checking what unit is cursor pointing at

                #region Collecting information about what cursor is hovering over
                RaycastHit hit;
                Vector3 vect = Input.mousePosition;
                vect.z = 999999;
                Vector3 cPos = Camera.main.ScreenToWorldPoint(vect);
                Physics.Raycast(Camera.main.transform.position, cPos, out hit, UnitPlacementBag.Instance.CellMask);
                Debug.DrawRay(Camera.main.transform.position, cPos, Color.red);
                #endregion
                if (hit.transform != null)
                {
                    Unit u = GetUnitWithTransform(pulledUnits, hit.transform);
                    Debug.Log(u);
                    if (u != null)
                    #region If hovering over a unit
                    {
                        u.Anim.SetBool(IsRaisedStr, true);
                        foreach (var v in pulledUnits) { if (v != u) { v.Anim.SetBool(IsRaisedStr, false); } }

                        //Waiting until player clicks mouse button to select a unit
                        if (Input.GetMouseButtonDown(0)) //TODO: Idk if I should replace it with the new input system, so far this works better
                        {
                            IsSelecting = false;
                            selectedUnit = u;
                        }
                    }
                    #endregion
                    #region If not hovering over a unit
                    else
                    {
                        foreach (var v in pulledUnits) { v.Anim.SetBool(IsRaisedStr, false); }
                    }
                }
                else
                {
                    foreach (var v in pulledUnits) { v.Anim.SetBool(IsRaisedStr, false); }
                }
                #endregion

                #endregion
            }
            #endregion
            #region If looking somewhere else
            else
            {
                SetPos(UnitPlacementBag.Instance.UnitsPos_Below, pulledUnits);
            }
            #endregion       

            
            
        }
        #endregion

        CameraManager.Instance.SetCam(CameraManager.Instance.TopDown);

        ActionParameters parms = new ActionParameters(GameManager.ActionType.PlayerCreate,
            new List<Unit> { selectedUnit }, null, null, 0);
        yield return Action_PlayerCreate.PlayerCreateOld(parms);

        selectedUnit.Anim.SetBool(IsRaisedStr, false);
        selectedUnit.Anim.SetBool(IsHoveringStr, false);

        yield return new WaitForSeconds(0.1f * Time.deltaTime);
        GameManager.Instance.RemoveAction(parameters);

    }

}
