using System.Collections.Generic;
using UnityEngine;

public class BaseGroupPathFinding : MonoBehaviour
{
    protected World world;
    protected PathFinding pathFinding;
    protected List<PathRoute> teamPathRoutes = new List<PathRoute>();
    public bool isActivePathFinding;

    protected virtual void Start()
    {
        world = WorldGeneration.instance.world;
        pathFinding = new PathFinding(world);
    }

    protected void PathfindingMoveToTarget()
    {
        if (teamPathRoutes.Count == 0) return;

        for (int i = 0; i < teamPathRoutes.Count; i++)
        {
            if (teamPathRoutes[i].pathIndex != -1)
            {
                Vector3 nextPathPosition = teamPathRoutes[i].pathRouteList[teamPathRoutes[i].pathIndex];
                Vector3 currentPos = teamPathRoutes[i].unitCharacter.transform.position;
                Vector3 direction = (nextPathPosition - currentPos).normalized;

                teamPathRoutes[i].unitCharacter.FacingDirection(direction);
                teamPathRoutes[i].unitCharacter.transform.position = Vector3.MoveTowards(currentPos, nextPathPosition, 5 * Time.deltaTime);

                if (Vector3.Distance(teamPathRoutes[i].unitCharacter.transform.position, nextPathPosition) <= 0.1f)
                {
                    teamPathRoutes[i].unitCharacter.transform.position = nextPathPosition;
                    teamPathRoutes[i].pathIndex++;
                    if (teamPathRoutes[i].pathIndex >= teamPathRoutes[i].pathRouteList.Count)
                    {
                        Debug.Log($"Reached target {teamPathRoutes[i].targetPosition}");
                        teamPathRoutes[i].pathIndex = -1;
                    }
                }
            }
        }
    }

    protected void AllUnitsToTarget()
    {
        if (isActivePathFinding)
        {
            for (int i = 0; i < teamPathRoutes.Count; i++)
            {
                if (teamPathRoutes[i].pathIndex != -1) { return; }
            }
            teamPathRoutes.Clear();
            isActivePathFinding = false;
        }
    }

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

    #region Reset
    public void ResetTeamPathRoute()
    {
        teamPathRoutes.Clear();
    }
    #endregion
}

