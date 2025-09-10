using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float Sensitivity;
    public float Speed;

    public float MaxSpeed;

    float mY;
    float mX;

    const float offset = 20;

    Camera cam;
    CharacterController cc;

    void Start()
    {
        cam = Camera.main;
        cc = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        Vector3 dir = cam.transform.forward * y + cam.transform.right * x; dir.y = 0;
        dir.x = Mathf.Clamp(dir.x, -MaxSpeed, MaxSpeed);
        dir.z = Mathf.Clamp(dir.z, -MaxSpeed, MaxSpeed);

        cc.Move(dir * Speed * Time.fixedDeltaTime);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        mY = Mathf.Clamp(mY - Input.GetAxis("Mouse Y") * Sensitivity * Time.fixedDeltaTime * offset, -90, 90);
        mX += Input.GetAxis("Mouse X") * Sensitivity * Time.fixedDeltaTime * offset;

        cam.transform.rotation = Quaternion.Euler(mY, mX, 0);
    }
}
