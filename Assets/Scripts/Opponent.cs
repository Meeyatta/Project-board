using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Opponent")]
public class Opponent : ScriptableObject
{
    public string Name;

    public List<UnitAmount> Army = new List<UnitAmount>();
}
