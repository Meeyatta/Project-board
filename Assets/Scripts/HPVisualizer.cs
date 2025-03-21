using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;


//This is applied to bases of all units and visualize their current health 
public class HPVisualizer : MonoBehaviour
{
    public TextMeshProUGUI hpText;
    public Slider hpSlider;
    Unit thisUnit;
    void Start()
    {
        thisUnit = transform.parent.parent.parent.GetComponent<Unit>();
    }

    // Update is called once per frame
    void Update()
    {
        if (thisUnit != null && hpText != null && hpSlider != null)
        {
            hpText.text = thisUnit.CurrentHealth + "/" + thisUnit.MaxHealth;
            hpSlider.value = thisUnit.CurrentHealth / thisUnit.MaxHealth;
        }
    }
}
