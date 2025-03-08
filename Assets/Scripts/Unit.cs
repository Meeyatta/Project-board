using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//CellGuide - list of lines which hold coordinats relative to the unit, used for moveset and attack zones

[System.Serializable]
public class Unit : MonoBehaviour
{
    public string UnitName;
    public UnitSize Size;
    public Moveset CurMoveset;
    public AttackZone CurAttackZone;
    public int MaxHealth;
    public int CurrentHealth;

    [Header("Abilities:")]
    #region Abilities
    public List<Ability> BaseAbilities = new List<Ability>(); //Abilities unit has as a base, these are rarely changed
    public HashSet<Ability> CondAbilities = new HashSet<Ability>(); //Abilities conditionally applied to unit during the game, these change often 
    public HashSet<Ability> CurAbilities = new HashSet<Ability>(); //Combination of Base abilities and Conditional Abilities
    #endregion

    [Header("Keywords:")]
    #region Keywords
    public List<Keyword> BaseKeywords = new List<Keyword>();
    public HashSet<Keyword> CondKeywords = new HashSet<Keyword>();
    public HashSet<Keyword> CurKeywords = new HashSet<Keyword>();
    #endregion

    [Header("----Read only information----")]
    public bool Moved = false;
    [Header("---------")]
    public GameObject UnitModelShowcase; //This is used to showcase where the unit model COULD be placed with move of creation
    public Vector3 ModelOffset;

    public enum Ability { Score, Invulnerable }
    public enum Keyword { Enemy, Player, Objective, Neutral };
    [HideInInspector] public Animator Anim;

    private void Awake()
    {
        Anim = GetComponent<Animator>();
    }
    void Start()
    {
        CurrentHealth = MaxHealth;

        if (UnitModelShowcase != null) 
        {
            UnitModelShowcase = Instantiate(UnitModelShowcase);
            UnitModelShowcase.SetActive(false);
        }
        else { Debug.Log(UnitName + " " + gameObject.name + " HAS NO UnitModelShowcase"); }
        
    }

    void UpdateInfo()
    {
        var curA = new HashSet<Ability>();
        curA.AddRange(BaseAbilities);
        curA.AddRange(CondAbilities);
        CurAbilities = curA;

        var curK = new HashSet<Keyword>();
        curK.AddRange(BaseKeywords);
        curK.AddRange(CondKeywords);
        CurKeywords = curK;

        //foreach (var k in CurKeywords)
        //{
        //    Debug.Log(k);
        //}
        //foreach (var k in CurAbilities)
        //{
        //    Debug.Log(k);
        //}

    }
    void FixedUpdate()
    {
        UpdateInfo();
    }
}
