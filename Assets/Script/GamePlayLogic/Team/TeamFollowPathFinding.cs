using System.Collections.Generic;
using UnityEngine;

public class TeamFollowPathFinding : BaseGroupPathFinding
{
    public static TeamFollowPathFinding instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    protected override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        AllUnitsToTarget();
        PathfindingMoveToTarget();
    }

    #region Sort
    //  Summary
    //      Sort the followerVectorRange based on distance to unit position
    private List<Vector3Int> SortTargetRangeByDistance(Vector3Int from, List<Vector3Int> targets)
    {
        var sorted = new List<Vector3Int>(targets);
        sorted.Sort((a, b) => Vector3.Distance(from, a).CompareTo(Vector3.Distance(from, b)));
        return sorted;
    }
    #endregion

    #region Team pathfinding
    public void TeamSortPathFinding(List<TeamFollower> teamFollowers, int spacing)
    {
        ResetTeamPathRoute();

        Vector3Int lastTargetPosition = Utils.RoundXZFloorYInt(teamFollowers[0].unitCharacter.transform.position);

        for (int i = 1; i < teamFollowers.Count; i++)
        {
            Vector3Int fromPosition = Utils.RoundXZFloorYInt(teamFollowers[i].unitCharacter.transform.position);

            if (IsWithinFollowRange(fromPosition, lastTargetPosition))
            {
                List<Vector3Int> unitRange = world.GetManhattas3DRange(lastTargetPosition, 2);
                teamPathRoutes.Add(new TeamPathRoute
                {
                    targetRangeList = unitRange,
                    unitCharacter = teamFollowers[i].unitCharacter,
                });
            }
            else
            {
                Debug.Log($"Target {lastTargetPosition} is too far from {fromPosition}");
                ResetTeamPathRoute();
                return;
            }

            bool foundPath = IsClosestTargetExist(fromPosition, teamPathRoutes[i - 1]);
            if (!foundPath)
            {
                Debug.Log($"No path found from {fromPosition} to {lastTargetPosition} break!");
                ResetTeamPathRoute();
                return;
            }
            lastTargetPosition = teamPathRoutes[i - 1].targetPosition.Value;
        }

        if (IsTeamSortPathAvaliable())
        {
            for (int i = 0; i < teamPathRoutes.Count; i++)
            {
                teamPathRoutes[i].pathIndex = 0;
            }
            isActivePathFinding = true;
        }
    }

    private bool IsWithinFollowRange(Vector3Int fromPosition, Vector3Int targetPosition, float maxDistance = 16f)
    {
        return Vector3.Distance(fromPosition, targetPosition) <= maxDistance;
    }

    private bool IsClosestTargetExist(Vector3Int fromPosition, TeamPathRoute teamPathRoute)
    {
        if (teamPathRoute.targetRangeList == null || teamPathRoute.targetRangeList.Count == 0)
            return false;

        var sortedTarget = SortTargetRangeByDistance(fromPosition, teamPathRoute.targetRangeList);

        for (int i = 0; i < sortedTarget.Count; i++)
        {
            List<Vector3> pathVectorList = pathFinding.GetPathRoute(fromPosition, sortedTarget[i]).pathVectorList;

            bool existSameTarget = IsTargetPositionExist(sortedTarget[i]);
            if (existSameTarget == true)
            {
                Debug.Log($"Target {sortedTarget[i]} is already exist");
                continue;
            }

            if (pathVectorList.Count != 0)
            {
                teamPathRoute.pathRouteList = pathVectorList;
                teamPathRoute.targetPosition = sortedTarget[i];
                Debug.Log($" {fromPosition} to target {teamPathRoute.targetPosition}");
                return true;
            }
        }
        return false;
    }

    private bool IsTeamSortPathAvaliable()
    {
        if (teamPathRoutes.Count == 0) return false;

        for (int i = 0; i < teamPathRoutes.Count; i++)
        {
            if (!teamPathRoutes[i].targetPosition.HasValue ||
                teamPathRoutes.Count == 0)
            {
                return false;
            }
        }
        return true;
    }
    #endregion

    private void OnDrawGizmos()
    {
        if (teamPathRoutes.Count == 0) return;
        for (int i = 0; i < teamPathRoutes.Count; i++)
        {
            if (teamPathRoutes[i].targetPosition.Value == null) return;

            Gizmos.color = Color.red;
            Gizmos.DrawCube(teamPathRoutes[i].targetPosition.Value, Vector3.one);
        }
    }
}