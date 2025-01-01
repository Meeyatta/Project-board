using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public Vector2 Size;
    public GameObject Tile;
    [SerializeField] Grid Grid_;
    private void Awake()
    {
        Grid_ = GetComponent<Grid>();
    }
    void BuildBoard()
    {
        for (int x = 0; x < Size.x; x++)
        {
            for (int y = 0; y < Size.y; y++)
            {
                var pos = Grid_.GetCellCenterWorld(new Vector3Int(x, y));
                Instantiate(Tile, pos, Quaternion.identity);
            }
        }
    }
    void Start()
    {
        
        BuildBoard();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
