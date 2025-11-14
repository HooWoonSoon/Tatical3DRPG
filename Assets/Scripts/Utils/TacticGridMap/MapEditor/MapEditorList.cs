using System.Collections.Generic;
using UnityEngine;

public class MapEditorList : MonoBehaviour
{
    public List<MapEditorLevelList> allMapList;

    public void AddMap(MapEditorLevelList map)
    {
        allMapList.Add(map);
        ResetMapList();
    }

    private void ResetMapList()
    {
        allMapList.RemoveAll(item => item == null);
    }
}
