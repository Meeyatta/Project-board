using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Item_WaterBucket
{
    static List<int> Size = new List<int> { 3, 3 };
    //Activate it

    //Start drawing the placement effect

    //Await until player clicks (Can cancel)

    //After player clicks on an acceptable position, apply the water effect to all applicable cells

    //Remove the item from current and end the action

    public static IEnumerator WaterBucket(ActionParameters parameters)
    {
        yield return new WaitForSeconds(Time.deltaTime);
        Debug.Log("WATER BUCKET, RELEASE");
        yield return Bucket.CoverArea(CoverType.Water, Size);

        yield return new WaitForSeconds(Time.fixedDeltaTime);
        GameManager.Instance.RemoveAction(parameters);
    }

}
