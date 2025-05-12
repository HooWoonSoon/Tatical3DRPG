using System.Collections.Generic;
using UnityEngine;

public class TeamFollowPathFinding : MonoBehaviour
{
    private World world;
    private TeamFollowSystem teamFollowSystem;
    private PathFinding pathFinding;

    private List<Vector3> pathVectorRange;
    private List<Vector3> followerVectorRange;

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

    public void TeamFollowPathFindingRange(Vector3Int unitPosition, int distanceRange)
    {
        pathVectorRange = GetManhattas3DRange(unitPosition, distanceRange, true);
    }

    //  Summary
    //      This function calculates the Manhattan distance range in 3D space
    private List<Vector3> GetManhattas3DRange(Vector3Int unitPosition, int size, bool checkWalkable)
    {
        List<Vector3> coverage = new List<Vector3>();

        for (int x = unitPosition.x - size; x <= unitPosition.x + size; x++)
        {
            for (int y = unitPosition.y - size; y <= unitPosition.y + size; y++)
            {
                for (int z = unitPosition.z - size; z <= unitPosition.z + size; z++)
                {
                    // Check if the current position is within the Manhattan distance range
                    if (Mathf.Abs(unitPosition.x - x) + Mathf.Abs(unitPosition.y - y) + Mathf.Abs(unitPosition.z - z) <= size)
                    {
                        if (world.IsValidNode(x, y, z) == false) continue;

                        if (checkWalkable == true)
                        {
                            if (world.GetNodeAtWorldPosition(x, y, z).isWalkable == false) continue;
                        }
                        coverage.Add(new Vector3(x, y, z));
                    }
                }
            }
        }
        return coverage;
    }

    public void CheckWithinRange(UnitCharacter targetFollower)
    {
        bool inRange;
        inRange = CheckWithinManhattasRange(Utils.RoundXZFloorYInt(targetFollower.transform.position));
        Debug.Log($"{targetFollower.index} is in range = {inRange}");
    }

    private bool CheckWithinManhattasRange(Vector3Int unitPosition)
    {
        for (int i = 0; i < pathVectorRange.Count; i++)
        {
            if (pathVectorRange[i] == unitPosition)
            {
                return true;
            }
        }
        return false;
    }

    public void FindClosetRange(UnitCharacter targetFollower)
    {
        if (pathVectorRange == null) { return; }
        followerVectorRange = GetClosetRange(targetFollower, 1);
    }

    private List<Vector3> GetClosetRange(UnitCharacter targetFollower, int size)
    {
        List<Vector3> coverage = new List<Vector3>();
        Vector3Int originPosition = Utils.RoundXZFloorYInt(targetFollower.transform.position);

        coverage = GetManhattas3DRange(originPosition, size, true);
        
        for (int i = 0; i < coverage.Count; i++)
        {
            if (pathVectorRange.Contains(coverage[i]) == false)
            {
                coverage.RemoveAt(i);
            }
        }
        return coverage;
    }

    public void TeamMemberRefinding(List<TeamFollower> teamFollowers, List<Vector3> targetPosition, out List<Vector3> pathTargetPos)
    {
        pathTargetPos = new List<Vector3>();
        for (int i = 0; i < teamFollowers.Count; i++)
        {
            List<Vector3> pathVectorList = pathFinding.GetPathRoute(Utils.RoundXZFloorYInt(teamFollowers[i].unitCharacter.transform.position), targetPosition[i]).pathVectorList;
            for (int j = 0; j < pathVectorList.Count - 1; j++)
            {
                Debug.DrawLine(pathVectorList[j], pathVectorList[j + 1], Color.red, 5f);
            }
            
            if (pathVectorList.Count != 0)
            {
                Vector3 finalTargetPosition = pathVectorList[pathVectorList.Count - 1];
                pathTargetPos.Add(finalTargetPosition);
            }

            if (pathVectorList.Count == 0)
            {
                Debug.Log($"{teamFollowers[i].unitCharacter.index} couldn't found path");
                Debug.Log("Change re-finding another path");
                targetPosition[i] = pathTargetPos[i - 1] - Vector3.back * 2f;
                List<Vector3> pathRefindVectorList = pathFinding.GetPathRoute(teamFollowers[i].unitCharacter.transform.position, targetPosition[i]).pathVectorList;
                for (int j = 0; j < pathRefindVectorList.Count - 1; j++)
                {
                    Debug.DrawLine(pathRefindVectorList[j], pathRefindVectorList[j + 1], Color.green, 5f);
                }

                if (pathRefindVectorList.Count != 0)
                {
                    Vector3 finalTargetPosition = pathRefindVectorList[pathRefindVectorList.Count - 1];
                    pathTargetPos.Add(finalTargetPosition);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (pathVectorRange == null) return;
        for (int i = 0; i < pathVectorRange.Count; i++)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(pathVectorRange[i], Vector3.one);
        }

        if (followerVectorRange == null) return;
        for (int i = 0; i < followerVectorRange.Count; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(followerVectorRange[i], Vector3.one);
        }
    }
}