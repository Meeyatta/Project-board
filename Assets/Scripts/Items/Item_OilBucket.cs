using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Item_OilBucket 
{
    static List<int> Size = new List<int> { 3, 3 };

    public static IEnumerator OilBucket(ActionParameters parameters)
    {
        yield return new WaitForSeconds(Time.deltaTime);
        //Debug.Log("OILED UP");
        yield return Bucket.CoverArea(EffectManager.Tag.Creation, CoverType.Oil, Size);

        yield return new WaitForSeconds(Time.fixedDeltaTime);
        GameManager.Instance.RemoveAction(parameters);
    }
}
