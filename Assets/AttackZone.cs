using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack zone", menuName = "Attack zone")]
public class AttackZone : ScriptableObject
{
    [System.Serializable]
    public class Line
    {
        public string Direction;
        public List<Vector2Int> Positions;
        public bool IsEvading;
    }
    public int Damage;
    public List<Line> Lines;
}
