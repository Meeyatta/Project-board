using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitPlacementBag : MonoBehaviour
{
    public Animator Anim;
    public GameObject UnitsPos_InFront; //Position in front of camera when we select units
    public GameObject UnitsPos_Below; //Position sitting on the board when player looks away
    const string isPullingStr = "isPulling";
    public static UnitPlacementBag Instance;
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
        Anim = GetComponent<Animator>();
        Singleton();
    }

    public void PullOutUnits()
    {
        Anim.SetBool(isPullingStr, true);
    }
    public void StopPullingUnits()
    {
        Anim.SetBool(isPullingStr, false);
    }
}
