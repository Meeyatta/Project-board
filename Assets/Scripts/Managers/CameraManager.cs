using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class CameraManager : MonoBehaviour
{
    [System.Serializable]
    public class CameraPos
    {
        public Vector3 Position;
        public Quaternion Rotation;
    }

    public CameraPos Pos1;
    public CameraPos Pos2;
    public CameraPos Pos3;
    public CameraPos Pos4;
    Camera cam;
    void Awake()
    {
        cam = Camera.main;
    }
    public void Swap_Pos1(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            cam.transform.localPosition = Pos1.Position;
            cam.transform.localRotation = Pos1.Rotation;
        }
    }
    public void Swap_Pos2(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            cam.transform.localPosition = Pos2.Position;
            cam.transform.localRotation = Pos2.Rotation;
        }
    }
    public void Swap_Pos3(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            cam.transform.localPosition = Pos3.Position;
            cam.transform.localRotation = Pos3.Rotation;
        }
    }
    public void Swap_Pos4(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            cam.transform.localPosition = Pos4.Position;
            cam.transform.localRotation = Pos4.Rotation;
        }
    }

}
