using System.Collections.Generic;
using UnityEngine;

public class MapEditorList : MonoBehaviour
{
    public List<GameObject> allMapList;

    public void AddMap(GameObject map)
    {
        allMapList.Add(map);
        ResetMapList();
    }

    private void ResetMapList()
    {
        allMapList.RemoveAll(item => item == null);
    }
}
