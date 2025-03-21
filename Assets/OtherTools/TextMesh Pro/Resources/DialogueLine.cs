using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Lang { ENG, RU, }

[CreateAssetMenu(fileName = "New Dialogue Line", menuName = "Dialogue")]
public class DialogueLine : ScriptableObject
{
    public Lang Language;
    public string Name;
    public string Text;
}
