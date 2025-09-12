using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandleLight_rotation : MonoBehaviour
{
    Camera Cam;

    void Start()
    {
        Cam = Camera.main;
    }

    void Update()
    {
        Vector3 newCamPos = Cam.transform.position; newCamPos.y = transform.position.y;
        Vector3 lookVect = newCamPos - transform.position; lookVect.Normalize();

        transform.forward = lookVect;
    }
}
