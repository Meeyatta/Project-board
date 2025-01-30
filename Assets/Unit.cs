using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Unit : MonoBehaviour
{
    public string UnitName;
    public Moveset CurMoveset;
    public Vector3 ModelOffset;

    public Animator Anim;

    private void Awake()
    {
        Anim = GetComponent<Animator>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
