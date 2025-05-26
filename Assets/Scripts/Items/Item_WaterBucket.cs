using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Item_WaterBucket
{
    //Activate it

    //Start drawing the placement effect

    //Await until player clicks (Can cancel)

    //After player clicks on an acceptable position, apply the water effect to all applicable cells

    //Remove the item from current and end the action

    public static IEnumerator WaterBucket(ActionParameters parameters)
    {
        yield return new WaitForSeconds(Time.deltaTime);
    }

}
