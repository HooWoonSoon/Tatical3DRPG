using System.Collections.Generic;
using UnityEngine;

public class PathRoute
{
    public PlayerCharacter character;
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
            pathRouteList.Add(pathNode.GetWorldVectorFromPath() + new Vector3(0, offset, 0));
    }
}