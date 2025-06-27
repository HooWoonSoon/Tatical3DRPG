using System.Collections.Generic;
using UnityEngine;

public class BaseGroupPathFinding : Entity
{
    protected List<PathRoute> teamPathRoutes = new List<PathRoute>();
    public bool isActivePathFinding;

    protected bool IsTargetPositionExist(Vector3 targetPosition)
    {
        for (int i = 0; i < teamPathRoutes.Count; i++)
        {
            if (teamPathRoutes[i].targetPosition.HasValue &&
                targetPosition == teamPathRoutes[i].targetPosition.Value)
            {
                return true;
            }
        }
        return false;
    }
}

