using System.Collections.Generic;
using UnityEngine;

public class PathRoute
{
    public CharacterBase character;
    public List<Vector3Int> targetRangeList;
    public List<Vector3> pathRouteList;
    public Vector3Int? targetPosition;
    public int pathIndex = -1;
    private float offset = 0.5f;

    public PathRoute() { }

    public PathRoute(List<GameNode> pathNodeList, Vector3 worldOrigin)
    {
        pathRouteList = new List<Vector3>();
        foreach (GameNode pathNode in pathNodeList)
            pathRouteList.Add(pathNode.GetVector() + new Vector3(0, offset, 0));
        if (pathRouteList.Count > 0)
        {
            pathIndex = 0;
        }
    }

    public void DebugPathRoute()
    {
        if (pathRouteList == null || pathRouteList.Count == 0)
        {
            Debug.Log("PathRoute is empty");
            return;
        }

        string pathLog = string.Join(" -> ", pathRouteList.ConvertAll(p => p.ToString()));
        Debug.Log($"{character} to PathTarget: {targetPosition},  PathRoute: {pathLog}");
    }
}