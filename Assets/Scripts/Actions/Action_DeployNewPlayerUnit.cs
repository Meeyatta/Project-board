using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Action_DeployNewPlayerUnit 
{
    static int PulledNumber = 3;
    public static IEnumerator DeployNewUnit(ActionParameters parameters)
    {
        yield return new WaitForSeconds(Time.deltaTime);

        GameObject unitsPoint = UnitPlacementBag.Instance.NewUnitsPosObj;

        //Change the camera position to look at the position in front of the board
        CameraManager.Instance.SetCam(CameraManager.Instance.InFrontOfBoad);

        //Play the animation of bag before units are pulled out

        //Select up to 3 random not-placed units  
        List<Unit> shuffled = ResourceManager.Instance.ShuffleList(ResourceManager.Instance.Army_NotPlaced);
        List<Unit> pulledUnits = new List<Unit>();
        for (int i = 0; i < Mathf.Min(PulledNumber, shuffled.Count); i++)
        {
            pulledUnits.Add(shuffled[i]);
            Debug.Log("Pulled out a " + shuffled[i]);
        }

        Debug.Log("Position is " + unitsPoint.transform.position);

        foreach (Unit unit in pulledUnits) 
        {
            unit.transform.position = unitsPoint.transform.position;
        }

        //Show these untis to the player

        //Lock the camera position and wait until player clicks on one of them and confirms the choice

        //Swap the camera position to the top view and allow the player to place the selected unit

        //Hide the other units back into not-placed

        yield return new WaitForSeconds(0.1f * Time.deltaTime);
        GameManager.Instance.RemoveAction(parameters);

    }

}
