using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Item_WaterBucket
{
    static List<int> Size = new List<int> { 3, 3 };

    public static IEnumerator WaterBucket(ActionParameters parameters)
    {
        yield return new WaitForSeconds(Time.deltaTime);
        //Debug.Log("WATER BUCKET, RELEASE");
        yield return Bucket.CoverArea(EffectManager.Tag.Creation, CoverType.Water, Size);

        yield return new WaitForSeconds(Time.fixedDeltaTime);
        GameManager.Instance.RemoveAction(parameters);
    }

}
