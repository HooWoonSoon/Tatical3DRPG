using System.Collections.Generic;
using UnityEngine;

public class PathRoute
{
    public List<Vector3> pathVectorList;

    public PathRoute(List<GameNode> pathNodeList, Vector3 worldOrigin)
    {
        pathVectorList = new List<Vector3>();
        foreach (GameNode pathNode in pathNodeList)
            pathVectorList.Add(pathNode.GetWorldVectorFromPath());
    }
}