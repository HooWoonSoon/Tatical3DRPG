
using System.Collections.Generic;
using UnityEngine;

public class TeamSystem : Entity
{
    public List<PathRoute> teamPathRoutes = new List<PathRoute>();

    //public List<PathRoute> GetGridBattlePath(List<CharacterBase> characters)
    //{
    //    List<PathRoute> pathRoutes = new List<PathRoute>();

    //    for (int i = 0; i < characters.Count; i++)
    //    {
    //        Vector3Int unitPosition = characters[i].GetCharacterPosition();

    //        int minSize = 1;
    //        int maxSize = 8;
    //        bool found = false;

    //        while (!found && minSize <= maxSize)
    //        {
    //            List<Vector3Int> range = world.GetManhattas3DGameNodePosition(unitPosition, minSize);
    //            range = Utils.SortTargetRangeByDistance(unitPosition, range);
    //            for (int j = 0; j < range.Count; j++)
    //            {
    //                if (IsTargetPositionExist(pathRoutes, range[j])) { continue; }

    //                List<Vector3> pathVectorList = (pathFinding.GetPathRoute(unitPosition, range[j], 1, 1).pathRouteList);

    //                if (pathVectorList.Count != 0)
    //                {
    //                    pathRoutes.Add(new PathRoute
    //                    {
    //                        character = characters[i],
    //                        targetPosition = range[j],
    //                        pathRouteList = pathVectorList,
    //                        pathIndex = 0
    //                    });
    //                    found = true;
    //                    break;
    //                }
    //            }
    //            minSize++;
    //        }

    //        if (!found)
    //        {
    //            Debug.Log("Not found any path");
    //            pathRoutes.Add(new PathRoute
    //            {
    //                character = characters[i],
    //                targetPosition = unitPosition,
    //                pathRouteList = new List<Vector3> { unitPosition },
    //                pathIndex = 0
    //            });
    //        }
    //    }

    //    if (pathRoutes.Count != characters.Count) 
    //    { 
    //        Debug.LogError("Map design error");
    //        return null;
    //    }

    //    return pathRoutes;
    //}

    //public bool IsTargetPositionExist(Vector3 targetPosition)
    //{
    //    for (int i = 0; i < teamPathRoutes.Count; i++)
    //    {
    //        if (teamPathRoutes[i].targetPosition.HasValue &&
    //            targetPosition == teamPathRoutes[i].targetPosition.Value)
    //        {
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    public bool IsTargetPositionExist(PathRoute pathRoute, Vector3 targetPosition)
    {
        if (pathRoute.targetPosition.HasValue &&
            targetPosition == pathRoute.targetPosition.Value)
        {
            return true;
        }
        return false;
    }
    //private void OnDrawGizmos()
    //{
        //if (teamPathRoutes.Count == 0) return;
        //for (int i = 0; i < teamPathRoutes.Count; i++)
        //{
        //    if (teamPathRoutes[i].targetPosition.Value == null) return;

        //    Gizmos.color = Color.red;
        //    Gizmos.DrawCube(teamPathRoutes[i].targetPosition.Value, Vector3.one);
        //}
    //}
}
