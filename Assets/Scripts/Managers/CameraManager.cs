using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class CameraManager : MonoBehaviour
{
    public GameObject CurPos;
    public GameObject FrontLower;
    public GameObject FrontUpper;
    public GameObject TopDown;
    public GameObject Right;
    public GameObject Left;

    Camera cam;
    void Awake()
    {
        cam = Camera.main;
    }

    public void SetCam(GameObject camm)
    {
        List<GameObject> all = new List<GameObject> { FrontLower, FrontUpper, TopDown, Right, Left };

        foreach (var v in all)
        {
            if (camm == v)
            {
                CurPos = v;
                v.SetActive(true);
            }
            else
            {
                v.SetActive(false);
            }
        }
    }

    #region Moving forwards/backwards
    public void Swap_Forward(InputAction.CallbackContext context)
    {
        if (FrontLower == null || FrontUpper == null || TopDown == null || Right == null || FrontLower == Left) return;

        if (context.performed)
        {
            if (CurPos == FrontUpper) 
            {
                SetCam(TopDown);
            }
            else if (CurPos != TopDown)
            {
                SetCam(FrontUpper);
            }
        }
    }
    public void Swap_Back(InputAction.CallbackContext context)
    {
        if (FrontLower == null || FrontUpper == null || TopDown == null || Right == null || FrontLower == Left) return;

        if (context.performed)
        {
            if (CurPos == TopDown)
            {
                SetCam(FrontUpper);
            }
            else if (CurPos != FrontLower)
            {
                SetCam(FrontLower);
            }
        }
    }
    #endregion


    #region Moving left/right
    public void Swap_Left(InputAction.CallbackContext context)
    {
        if (FrontLower == null || FrontUpper == null || TopDown == null || Right == null || FrontLower == Left) return;

        if (context.performed)
        {
            if (CurPos == Right)
            {
                SetCam(FrontUpper);
            }
            else if (CurPos != Left)
            {
                SetCam(Left);
            }
        }
    }
    public void Swap_Right(InputAction.CallbackContext context)
    {
        if(FrontLower == null || FrontUpper == null || TopDown == null || Right == null || FrontLower == Left) return;

        if (context.performed)
        {
            if (CurPos == Left)
            {
                SetCam(FrontUpper);
            }
            else if (CurPos != Right)
            {
                SetCam(Right);
            }
        }
    }
    #endregion
}
