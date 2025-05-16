using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TeamFollowPathFinding : MonoBehaviour
{
    public class TeamPathRoute
    {
        public List<Vector3Int> targetRangeList;
        public List<Vector3> pathRouteList;
        public Vector3Int? targetPosition;

        public int pathIndex = -1;
    }

    private World world;
    private TeamFollowSystem teamFollowSystem;
    private PathFinding pathFinding;

    private List<Vector3Int> pathVectorRange;
    private List<Vector3Int> followerVectorRange;

    private List<TeamPathRoute> teamPathRoutes = new List<TeamPathRoute>();

    public static TeamFollowPathFinding instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        world = WorldManager.instance.world;
        teamFollowSystem = TeamFollowSystem.instance;
        pathFinding = new PathFinding(world);
    }

    private void Update()
    {
        if (teamPathRoutes.Count == 0) return;

        for (int i = 1; i < TeamFollowSystem.instance.teamFollowers.Count; i++)
        {
            var follower = TeamFollowSystem.instance.teamFollowers[i];
            var route = teamPathRoutes[i - 1];

            if (route.pathIndex != -1 && route.pathRouteList != null && route.pathRouteList.Count > 0)
            {
                Vector3 nextPathPosition = route.pathRouteList[route.pathIndex];
                Vector3 currentPos = follower.unitCharacter.transform.position;
                Vector3 direction = (nextPathPosition - currentPos).normalized;

                Debug.Log("moveDirection:" + direction);

                follower.unitCharacter.FacingDirection(direction);

                follower.unitCharacter.transform.position = Vector3.MoveTowards(currentPos, nextPathPosition, 5 * Time.deltaTime);

                if (Vector3.Distance(follower.unitCharacter.transform.position, nextPathPosition) <= 0.1f)
                {
                    follower.unitCharacter.transform.position = nextPathPosition;
                    route.pathIndex++;

                    if (route.pathIndex >= route.pathRouteList.Count)
                    {
                        Debug.Log($"Reached target {route.targetPosition}");
                        route.pathIndex = -1;
                    }
                }
            }
        }
    }

    #region Reset
    public void ResetTeamPathRoute()
    {
        teamPathRoutes.Clear();
    }
    #endregion

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

    #region Manhattan Distance Logic
    //  Summary
    //      This function calculates the Manhattan distance range in 3D space
    private List<Vector3Int> GetManhattas3DRange(
        Vector3Int unitPosition,
        int size,
        bool checkWalkable,
        bool limitY = false,
        int yLength = 0)
    {
        List<Vector3Int> coverage = new List<Vector3Int>();

        if (limitY && size > yLength) { size = yLength; }

        int minX = unitPosition.x - size;
        int maxX = unitPosition.x + size;
        int minY = unitPosition.y - size;
        int maxY = unitPosition.y + size;
        int minZ = unitPosition.z - size;
        int maxZ = unitPosition.z + size;

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    // Check if the current position is within the Manhattan distance range
                    int manhattasDistance = Mathf.Abs(unitPosition.x - x)
                             + Mathf.Abs(unitPosition.y - y)
                             + Mathf.Abs(unitPosition.z - z);
                    if (manhattasDistance > size) continue;

                    if (!world.IsValidNode(x, y, z)) continue;

                    if (checkWalkable && !world.GetNodeAtWorldPosition(x, y, z).isWalkable)
                        continue;

                    coverage.Add(new Vector3Int(x, y, z));
                }
            }
        }
        return coverage;
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
                List<Vector3Int> unitRange = GetManhattas3DRange(lastTargetPosition, 2, true);
                teamPathRoutes.Add(new TeamPathRoute { targetRangeList = unitRange });
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

    private bool IsTargetPositionExist(Vector3 targetPosition)
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