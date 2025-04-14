using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Action_DeployNewPlayerUnit 
{
    static int PulledNumber = 3;
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
            pulledUnits.Add(shuffled[i]);
            Debug.Log("Pulled out a " + shuffled[i]);
        }

        //Show these untis to the player
        foreach (Unit unit in pulledUnits)
        {
            unit.transform.position = unitsPoint.transform.position;
        }

        //Lock the camera position and wait until player clicks on one of them and confirms the choice

        //Swap the camera position to the top view and allow the player to place the selected unit

        //Hide the other units back into not-placed

        yield return new WaitForSeconds(0.1f * Time.deltaTime);
        GameManager.Instance.RemoveAction(parameters);

    }

}
