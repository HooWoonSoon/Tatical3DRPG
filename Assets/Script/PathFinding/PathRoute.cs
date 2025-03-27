using System.Collections.Generic;
using UnityEngine;

public class PathRoute
{
    public List<Vector3> pathVectorList;
    private float offset = 0.5f;
    public PathRoute(List<GameNode> pathNodeList, Vector3 worldOrigin)
    {
        pathVectorList = new List<Vector3>();
        foreach (GameNode pathNode in pathNodeList)
            pathVectorList.Add(pathNode.GetWorldVectorFromPath() + new Vector3(0, offset, 0));
    }
}