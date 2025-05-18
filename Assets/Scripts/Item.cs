using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemId
{
    water,          //Covers 3x3 square in Water
    oil,            //Covers 3x3 square on Oil
    card,           //Makes a selected unit receive more damage
    toxin,          //Covers 3x3 square in Toxin

}

public class Item : MonoBehaviour
{
    public string Name;
    public ItemId Id;

    public Animator Anim;
    public Animator Animator_2D;

    [HideInInspector]
    public bool IsPrefab = true;

    void Awake()
    {
        IsPrefab = false;
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
