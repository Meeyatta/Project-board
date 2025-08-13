using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class A_Slippery : Ability
{
    public A_Slippery(Ability_Name n, string d, AbilityInstance i) : base(n, d, i)
    {
        aName = n;
        Description = d;
        Instance = i;
    }


    public float Delay = 12f;

    public List<Vector2Int> GetRandPos()
    {
        List<Vector2Int> res = new List<Vector2Int>();
        int dir = Random.Range(0, 8); //Get the direction in which unit slips
        Unit unit = Instance.Owner;

        switch (dir)
        {
            #region Up
            case 0:
                res = BoardManager.Instance.Get_UnitPositions(unit);
                for (int i = 0; i < res.Count; i++) { res[i] += new Vector2Int(0, -1); }
                break;
            #endregion
            #region Up-right
            case 1:
                res = BoardManager.Instance.Get_UnitPositions(unit);
                for (int i = 0; i < res.Count; i++) { res[i] += new Vector2Int(1, -1); }
                break;
            #endregion
            #region Right
            case 2:
                res = BoardManager.Instance.Get_UnitPositions(unit);
                for (int i = 0; i < res.Count; i++) { res[i] += new Vector2Int(1, 0); }
                break;
            #endregion
            #region Right-down
            case 3:
                res = BoardManager.Instance.Get_UnitPositions(unit);
                for (int i = 0; i < res.Count; i++) { res[i] += new Vector2Int(1, 1); }
                break;
            #endregion
            #region Down
            case 4:
                res = BoardManager.Instance.Get_UnitPositions(unit);
                for (int i = 0; i < res.Count; i++) { res[i] += new Vector2Int(0, 1); }
                break;
            #endregion
            #region Down-left
            case 5:
                res = BoardManager.Instance.Get_UnitPositions(unit);
                for (int i = 0; i < res.Count; i++) { res[i] += new Vector2Int(-1, 1); }
                break;
            #endregion
            #region Left
            case 6:
                res = BoardManager.Instance.Get_UnitPositions(unit);
                for (int i = 0; i < res.Count; i++) { res[i] += new Vector2Int(-1, 0); }
                break;
            #endregion
            #region Left-up
            default:
                res = BoardManager.Instance.Get_UnitPositions(unit);
                for (int i = 0; i < res.Count; i++) { res[i] += new Vector2Int(-1, -1); }
                break;
                #endregion
        }

        return res;
    }
    public IEnumerator Try()
    {
        yield return new WaitForSeconds(Delay * Time.fixedDeltaTime);

        AudioManager.Instance.Play(SoundName.Slip, Instance.Owner.transform);

        List<Vector2Int> endPoss = GetRandPos();
        bool IsValid = BoardManager.Instance.AreInBounds(endPoss) && !BoardManager.Instance.AreAnyOccupied(endPoss);
        if (IsValid)
        {
            //Debug.Log("Supposed to slip");

            List<Vector2Int> oldPos = BoardManager.Instance.Get_UnitPositions(Instance.Owner);
            foreach (Vector2Int v in oldPos)
            {
                BoardManager.Instance.Board[v.x].Cells[v.y].CurUnit = null;
            }

            yield return BoardManager.Instance.StartCoroutine(BoardManager.Instance.PlaceUnit(Instance.Owner, endPoss));
        }

        yield return new WaitForSeconds(Time.fixedDeltaTime);
    }

}
