using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class Action_DeployNewPlayerUnit
{
    public static UnityEvent<Unit> E_DeployedNewPlayerUnit = new UnityEvent<Unit>();

    static int PulledNumber = 3;
    static float SpaceBetweenUnits = 2;

    const string IsHoveringStr = "isHovering";
    const string IsRaisedStr = "isRaised";

    #region GetUnitWithTransform() - Finds a unit from a list using its transform
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

    #region SetPos () - Sets the pulled units beside the listed point
    static void SetPos(GameObject center, List<Unit> units)
    {
        for (int i = 0; i < units.Count; i++)
        {
            float half = (float)((units.Count - 1f) / 2f);
            float offsetMod = (half - i) * -1; //Offset modifier to the current unit

            Vector3 offset = new Vector3(SpaceBetweenUnits, 0, 0) * offsetMod;

            //Debug.Log(i + " " + offset);
            units[i].transform.position = center.transform.position + offset;

        }
    }
    #endregion

    #region IsFocused() - Checks if player is focused on units in front of camera or is looking somewhere else
    static bool IsFocused()
    {
        if (CameraManager.Instance.CurPos == CameraManager.Instance.InFrontOfBoad) return true;

        return false;
    }
    #endregion

    public static IEnumerator DeployNewUnit(ActionParameters parameters)
    {
        yield return new WaitForSeconds(Time.deltaTime);

        #region If we have no units to deploy
        if (ResourceManager.Instance.Army_NotPlaced.Count == 0)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            E_DeployedNewPlayerUnit.Invoke(null);
            GameManager.Instance.RemoveAction(parameters);
            yield break;
        }
        #endregion

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

        GameObject unitsPoint = UnitPlacementBag.Instance.UnitsPos_InFront;

        //Camera is drawn in front of the board
        CameraManager.Instance.SetCam(CameraManager.Instance.InFrontOfBoad);

        //The bag plays a shuffle animation and a sound
        UnitPlacementBag.Instance.PullOutUnits();

        //3 unit models hover in front of the player up and down 
        List<Unit> shuffled = ResourceManager.Instance.ShuffleList(ResourceManager.Instance.Army_NotPlaced);
        List<Unit> pulledUnits = new List<Unit>();
        for (int i = 0; i < Mathf.Min(PulledNumber, shuffled.Count); i++)
        {
            shuffled[i].gameObject.SetActive(true);
            pulledUnits.Add(shuffled[i]);
            //Debug.Log("Pulled out a " + shuffled[i]);
        }

        #region Hovering the possible units while player selects one
        Unit selectedUnit = null;
        foreach (var v in pulledUnits) { v.Anim.SetBool(IsHoveringStr, true); } //Making units hover 
        while (selectedUnit == null)
        {
            yield return new WaitForSeconds(Time.deltaTime);

            #region If player foces on new units
            if (IsFocused())
            {
                SetPos(UnitPlacementBag.Instance.UnitsPos_InFront, pulledUnits);

                #region Checking what unit is cursor pointing at
                RaycastHit hit;
                Vector3 vect = Input.mousePosition;
                vect.z = 999999;
                Vector3 cPos = Camera.main.ScreenToWorldPoint(vect);
                Physics.Raycast(Camera.main.transform.position, cPos, out hit);

                if (hit.transform != null)
                {

                    Unit u = GetUnitWithTransform(pulledUnits, hit.transform);
                    if (u != null)
                    #region If hovering over a unit - wait until we click on it
                    {
                        u.Anim.SetBool(IsRaisedStr, true);
                        foreach (var v in pulledUnits) { if (v != u) { v.Anim.SetBool(IsRaisedStr, false); } }

                        //Waiting until player clicks mouse button to select a unit
                        if (Input.GetMouseButtonDown(0)) //TODO: Idk if I should replace it with the new input system, so far this works better
                        {
                            selectedUnit = u;
                        }
                    }
                    #endregion
                    #region If not hovering over a unit
                    else
                    {
                        foreach (var v in pulledUnits) { v.Anim.SetBool(IsRaisedStr, false); }
                    }
                    #endregion
                }
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

        #region Hiding the unselected units
        foreach (var unit in pulledUnits)
        {
            if (selectedUnit != unit) { unit.gameObject.SetActive(false); }
            unit.Anim.SetBool(IsHoveringStr, false);
        }
        #endregion

        CameraManager.Instance.SetCam(CameraManager.Instance.TopDown);

        #region Placing the selected unit
        if (selectedUnit != null)
        {
            selectedUnit.Anim.SetBool(IsHoveringStr, false);
            selectedUnit.Anim.SetBool(IsRaisedStr, false);
            ResourceManager.Instance.RecordPlacedUnit(selectedUnit);

            GameManager.Instance.ShowDeploymentEvent.Invoke(new List<Unit> { selectedUnit });
            ActionParameters createPar = new ActionParameters(
                        GameManager.ActionType.PlayerCreate, new List<Unit> { selectedUnit }, null, null, null, 0);
            yield return Action_PlayerCreate.PlayerCreate(createPar);
            GameManager.Instance.HideDeploymentEvent.Invoke();
        }
        #endregion


        UnitPlacementBag.Instance.StopPullingUnits();

        E_DeployedNewPlayerUnit.Invoke(selectedUnit);
        yield return new WaitForSeconds(0.1f * Time.deltaTime);
        GameManager.Instance.RemoveAction(parameters);

    }

}
