using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardCell : MonoBehaviour
{
    public Vector2Int Coordinates = new Vector2Int(0,0);
    public void OnClick()
    {
        GameManager.Instance.ClickEvent.Invoke(Coordinates);
    }
}
