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
    public CameraPos CurPos;
    public List<CameraPos> ForwardPosses;
    public List<CameraPos> SidePosses;
    Camera cam;
    void Awake()
    {
        cam = Camera.main;
    }
    #region Moving forwards/backwards
    public void Swap_Forward(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (CurPos == ForwardPosses[0]) 
            {
                CurPos = ForwardPosses[1];

                cam.transform.localPosition = CurPos.Position;
                cam.transform.localRotation = ForwardPosses[1].Rotation;
            }
            else
            {
                CurPos = ForwardPosses[2];

                cam.transform.localPosition = CurPos.Position;
                cam.transform.localRotation = ForwardPosses[2].Rotation;
            }
        }
    }
    public void Swap_Back(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (CurPos == ForwardPosses[2])
            {
                CurPos = ForwardPosses[1];

                cam.transform.localPosition = CurPos.Position;
                cam.transform.localRotation = ForwardPosses[1].Rotation;
            }
            else
            {
                CurPos = ForwardPosses[0];

                cam.transform.localPosition = CurPos.Position;
                cam.transform.localRotation = ForwardPosses[0].Rotation;
            }
        }
    }
    #endregion


    #region Moving left/right
    public void Swap_Left(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (CurPos == SidePosses[1])
            {
                CurPos = ForwardPosses[1];

                cam.transform.localPosition = CurPos.Position;
                cam.transform.localRotation = ForwardPosses[1].Rotation;
            }
            else
            {
                CurPos = SidePosses[0];

                cam.transform.localPosition = CurPos.Position;
                cam.transform.localRotation = SidePosses[0].Rotation;
            }
        }
    }
    public void Swap_Right(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (CurPos == SidePosses[0])
            {
                CurPos = ForwardPosses[1];

                cam.transform.localPosition = CurPos.Position;
                cam.transform.localRotation = ForwardPosses[1].Rotation;
            }
            else
            {
                CurPos = SidePosses[1];

                cam.transform.localPosition = CurPos.Position;
                cam.transform.localRotation = SidePosses[1].Rotation;
            }
        }
    }
    #endregion
}
