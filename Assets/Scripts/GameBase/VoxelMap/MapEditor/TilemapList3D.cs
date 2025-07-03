using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Grid))]
public class TilemapList3D : MonoBehaviour
{
    private float initialHeight = 0;
    public List<GameObject> tilemapList = new List<GameObject>();

    public void AddLayer(GameObject layer)
    {
        tilemapList.Add(layer);
        ResetTileMapList();
    }

    public int LayerCount() { return tilemapList.Count; }

    //  Summary
    //      To avoid the empty list with no any game object may return the situation that null expected reference
    //      To reset the list while and avoid the invalid gameobject
    //
    public void ResetTileMapList()
    {
        for (int i = tilemapList.Count - 1; i >= 0; i--)
        {
            if (tilemapList[i] == null) { tilemapList.RemoveAt(i); }
            else
            {
                tilemapList[i].name = "Level (" + i + ")";
                Grid grid = GetComponent<Grid>();
                float levelHeight = i * grid.cellSize.y * grid.transform.localScale.y;
                tilemapList[i].transform.position = new Vector3(0, levelHeight + initialHeight, 0);
            }
        }
    }
}