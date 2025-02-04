using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//CellGuide - list of lines which hold coordinats relative to the unit, used for moveset and attack zones

[System.Serializable]
public class Unit : MonoBehaviour
{
    public string UnitName;
    public Moveset CurMoveset;
    public AttackZone CurAttackZone;
    public int MaxHealth;
    public int CurrentHealth;
    public List<Keyword> Keywords = new List<Keyword>();

    public Vector3 ModelOffset;

    public enum Keyword { Enemy, Player };
    [HideInInspector] public Animator Anim;

    private void Awake()
    {
        Anim = GetComponent<Animator>();
    }
    void Start()
    {
        CurrentHealth = MaxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
