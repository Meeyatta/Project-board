using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//CellGuide - list of lines which hold coordinats relative to the unit, used for moveset and attack zones
public enum Keyword { 
    Enemy,              //Units not belonging to the player
    Player,              //Units belonging to the player         
    Objective,          //Objectives used for scoring points
    Neutral             //Something controlled by neither the enemy nor the player
};
[System.Serializable]
public class Unit : MonoBehaviour
{
    public string UnitName;
    public UnitSize Size;
    public Moveset CurMoveset;
    public AttackZone CurAttackZone;
    public int MaxHealth;
    public int CodexPage;
    [Header("Abilities:")]
    #region Abilities
    public List<Ability> BaseAbilities = new List<Ability>(); //Abilities unit has as a base, these are rarely changed
    public HashSet<Ability> CondAbilities = new HashSet<Ability>(); //Abilities conditionally applied to unit during the game, these change often 
    public HashSet<Ability> CellCoverAbilities = new HashSet<Ability>(); //Abilities applied depending on what thing the cells are covered with
    public HashSet<Ability> CurAbilities = new HashSet<Ability>(); //Combination of Base abilities and Conditional Abilities
    #endregion
    
    [Header("Keywords:")]
    #region Keywords
    public List<Keyword> BaseKeywords = new List<Keyword>();
    public HashSet<Keyword> CondKeywords = new HashSet<Keyword>();
    public HashSet<Keyword> CurKeywords = new HashSet<Keyword>();
    #endregion

    [Header("Visuals:")]
    [HideInInspector]
    public Color CurColor = new Color(255, 255, 255);

    public Color PlayerColor = new Color(0, 255, 255);
    public Material PlayerBaseMat;

    public Color EnemyColor = new Color(255, 0, 0);
    public Material EnemyBaseMat;

    [Header("----Read only information----")]
    public GameObject FrontSprite;
    public GameObject TopSprite;

    public int CurrentHealth;
    public bool IsPrefab = true;
    GameObject Base;
    public bool Moved = false;
    [Header("---------")]
    public Outline BaseOutline;
    public Renderer BaseRend;

    public GameObject UnitModelShowcase; //This is used to showcase where the unit model COULD be placed with move of creation
    public Vector3 ModelOffset;

    [HideInInspector] public Animator Anim;

    private void OnEnable()
    {
        Anim = GetComponent<Animator>();

        Transform t = transform.Find("base");
        if (t != null) Base = t.gameObject;

        if (Base != null ) BaseOutline = Base.GetComponent<Outline>();
        if (Base != null) BaseRend = Base.GetComponent<Renderer>();
    }

    private void Awake()
    {
        Anim = GetComponent<Animator>();
        IsPrefab = false;

        Transform t = transform.Find("base");
        if (t != null) Base = t.gameObject;

        if (Base != null) BaseOutline = Base.GetComponent<Outline>();
        if (Base != null) BaseRend = Base.GetComponent<Renderer>();
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


    #region Turns this into a player unit
    
    public void SetToPlayer()
    {
        CurColor = PlayerColor;

        if (BaseRend != null) BaseRend.material = PlayerBaseMat;

        if (BaseKeywords.Contains(Keyword.Enemy)) BaseKeywords.Remove(Keyword.Enemy);
        if (!BaseKeywords.Contains(Keyword.Player)) BaseKeywords.Add(Keyword.Player);


    }
    #endregion 

    #region Turns this into an enemy unit
    public void SetToEnemy()
    {
        CurColor = EnemyColor;

        if (BaseRend != null) BaseRend.material = EnemyBaseMat;

        if (BaseKeywords.Contains(Keyword.Player)) BaseKeywords.Remove(Keyword.Player);
        if (!BaseKeywords.Contains(Keyword.Enemy)) BaseKeywords.Add(Keyword.Enemy);
    }
    #endregion

    void UpdateSprite()
    {
        if (TopSprite == null || FrontSprite == null) { Debug.Log(gameObject.name + " doesn't have one of their sprites"); return; }

        if (CameraManager.Instance.CurPos == CameraManager.Instance.TopDown)
        {
            TopSpriteRot();
        }
        else
        {
            FrontSpriteRot();
        }
    }
    #region Rotating the unit's sprite when looking from on top
    void FrontSpriteRot()
    {
        TopSprite.SetActive(false);
        FrontSprite.SetActive(true);

        Vector3 targetPos = Vector3.zero;

        if (CameraManager.Instance.CurPos == CameraManager.Instance.InFrontOfBoad)
        {
            targetPos = new Vector3(transform.position.x, transform.position.y, transform.position.z + 5);
        }
        else
        {
            targetPos = new Vector3(Camera.main.transform.position.x, transform.position.y, Camera.main.transform.position.z);
        }

        FrontSprite.transform.LookAt(targetPos, transform.up);
    }
    #endregion

    #region Rotating the unit's sprite when looking from anywhere else
    void TopSpriteRot()
    {
        TopSprite.SetActive(true);
        FrontSprite.SetActive(false);
    }
    #endregion

    public void PlaySound(SoundName name)
    {
        AudioManager.Instance.Play(name, transform);
    }
    void UpdateInfo()
    {
        var curA = new HashSet<Ability>();
        curA.AddRange(BaseAbilities);
        curA.AddRange(CondAbilities);
        curA.AddRange(CellCoverAbilities);

        CurAbilities = curA;

        var curK = new HashSet<Keyword>();
        curK.AddRange(BaseKeywords);
        curK.AddRange(CondKeywords);
        CurKeywords = curK;

        if (BaseOutline != null) BaseOutline.OutlineColor = CurColor;
        if (BaseOutline != null) BaseOutline.enabled = !Moved;

    }
    void FixedUpdate()
    {
        UpdateSprite();

        UpdateInfo();
    }
}
