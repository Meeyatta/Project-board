using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


//Handles visual effects in the game
public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;

    public GameObject CellOverlay;
    public GameObject Effects;

    #region Events 
    void Start()
    {
        Effects = GameObject.Find("Effects");
        GameManager.Instance.ShowMovementEvent.AddListener(ShowMovement);
    }
    void OnDisable()
    {
        GameManager.Instance.ShowMovementEvent.RemoveListener(ShowMovement);
    }
    #endregion Events
    void Singleton()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(this);
    }
    private void Awake()
    {
        Singleton();
    }

    void ShowMovement(List<Unit> units)
    {
        foreach (Unit u in units)
        {
            foreach (Vector2Int v in u.CurMoveset.Positions)
            {
                List<Vector2Int> offsetted = new List<Vector2Int>(); offsetted = BoardManager.Instance.Get_UnitPositions(u); if (offsetted.Count == 0) { return; }
                for (int i = 0; i < offsetted.Count; i++) { offsetted[i] += v; }

                Vector3? result = BoardManager.Instance.BoardToWorldPosition(offsetted); if (!result.HasValue) { return;}     

                Instantiate(CellOverlay, result.Value, Quaternion.identity, Effects.transform);
            }
        }
    }
    void HideMovement(List<Unit> units)
    {

    }
    
}
