using System.Collections.Generic;
using UnityEngine;

public class TeamPathRoute
{
    public Character unitCharacter;
    public List<Vector3Int> targetRangeList;
    public List<Vector3> pathRouteList;
    public Vector3Int? targetPosition;

    public int pathIndex = -1;
}
