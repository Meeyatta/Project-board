using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is applied to a "modelHolder" child on unit model and displays if unit is able to be moved or not
public class MovedVisualizer : MonoBehaviour
{
    Unit thisUnit;
    void Start()
    {
        thisUnit = transform.parent.parent.parent.parent.GetComponent<Unit>();
    }
    void FixedUpdate()
    {
        if (thisUnit.Moved)
        {
            foreach (Transform t in transform) 
            {
                t.gameObject.SetActive(false);
            }
        }
        else
        {
            foreach (Transform t in transform)
            {
                t.gameObject.SetActive(true);
            }
        }
    }
}
