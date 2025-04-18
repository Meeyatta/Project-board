using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class CameraManager : MonoBehaviour
{
    public bool IsOffset;

    public List<GameObject> CurPos;

    //These should have 2 objects each: One for normal, one for offset
    public List<GameObject> InFrontOfBoad;
    public List<GameObject> FrontUpper;
    public List<GameObject> TopDown;
    public List<GameObject> Right;
    public List<GameObject> Left;

    Camera cam;
    public static CameraManager Instance;
    void Singleton()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(this);
    }
    
    private void Awake()
    {
        CurPos = FrontUpper;
        Singleton();
        cam = Camera.main;
    }
    public void Offset()
    {
        IsOffset = !IsOffset;
        Debug.Log("Set offset to" + IsOffset);       
    }
    void Update()
    {
        if (IsOffset)
        {
            CurPos[0].SetActive(false);
            CurPos[1].SetActive(true);
        }
        else
        {
            CurPos[0].SetActive(true);
            CurPos[1].SetActive(false);
        }
    }

    #region Sets camera to the specific position
    public void SetCam(List<GameObject> camm)
    {
        CurPos[0].SetActive(false);
        CurPos[1].SetActive(false);

        CurPos = camm;
    }
    #endregion

    #region Moving forwards/backwards
    public void Swap_Forward(InputAction.CallbackContext context)
    {
        if (InFrontOfBoad == null || FrontUpper == null || TopDown == null || Right == null || InFrontOfBoad == Left) return;

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
        if (InFrontOfBoad == null || FrontUpper == null || TopDown == null || Right == null || InFrontOfBoad == Left) return;

        if (context.performed)
        {
            if (CurPos == TopDown)
            {
                SetCam(FrontUpper);
            }
            else if (CurPos != InFrontOfBoad)
            {
                SetCam(InFrontOfBoad);
            }
        }
    }
    #endregion

    #region Moving left/right
    public void Swap_Left(InputAction.CallbackContext context)
    {
        if (InFrontOfBoad == null || FrontUpper == null || TopDown == null || Right == null || InFrontOfBoad == Left) return;

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
        if(InFrontOfBoad == null || FrontUpper == null || TopDown == null || Right == null || InFrontOfBoad == Left) return;

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
