using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Codex : MonoBehaviour
{
    Animator Anim;

    public List<GameObject> Pages;
    public int CurPage;

    const string openedStr = "opened";

    public void PlaySound(SoundName name)
    {
        AudioManager.Instance.Play(name, transform);
    }

    public void Use(InputAction.CallbackContext c)
    {
        if (c.performed)
        {
            if (Anim.GetBool(openedStr))
            {
                Close();
            }
            else
            {
                Open();

                if (GameManager.Instance.CurrentPointedAtUnit != null)
                {
                    OpenPage(GameManager.Instance.CurrentPointedAtUnit.CodexPage);
                }
            }
        }
    }

    public void NextPage(InputAction.CallbackContext c)
    {
        if (c.performed && Anim.GetBool(openedStr))
        {
            OpenPage(CurPage + 1);
        }
    }

    public void PreviousPage(InputAction.CallbackContext c)
    {
        if (c.performed && Anim.GetBool(openedStr))
        {
            OpenPage(CurPage - 1);
        }
    }

    public void OpenPage(int p)
    {
        //Debug.Log("Opening on" + p);
        if (p < 0 || p >= Pages.Count) return;

        Pages[CurPage].SetActive(false);

        CurPage = p;
        Pages[CurPage].SetActive(true);
    }

    public void Open()
    {
        CameraManager.Instance.IsOffset = true;
        Anim.SetBool(openedStr, true);
    }
    public void Close()
    {
        CameraManager.Instance.IsOffset = false;
        Anim.SetBool(openedStr, false);
    }
    private void Awake()
    {
        Anim = GetComponent<Animator>();
    }
}
