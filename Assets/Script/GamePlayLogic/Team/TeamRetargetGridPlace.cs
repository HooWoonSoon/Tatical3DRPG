using System;
using System.Collections.Generic;
using UnityEngine;
public class TeamRetargetGridPlace : BaseGroupPathFinding
{
    private List<TeamPathRoute> teamPathRoutes = new List<TeamPathRoute>();
    private Action onReachTargetForBattle;
    public bool isActivatePathFinding { get; private set; }

    public static TeamRetargetGridPlace instance { get; private set; }

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
        if (!isActivatePathFinding) { return; }

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
                        teamPathRoutes[i].pathIndex = -1;
                    }
                }
            }
        }
        isActivatePathFinding = false;
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

    private List<Vector3Int> SortTargetRangeByDistance(Vector3Int from, List<Vector3Int> targets)
    {
        var sorted = new List<Vector3Int>(targets);
        sorted.Sort((a, b) => Vector3.Distance(from, a).CompareTo(Vector3.Distance(from, b)));
        return sorted;
    }

    public void EnterBattlePathFinding(List<Character> unitCharacters)
    {
        for (int i = 0; i < unitCharacters.Count; i++)
        {
            EnterBattlePathFinding(unitCharacters[i]);
        }
        isActivatePathFinding = true;
    }

    private void EnterBattlePathFinding(Character unitCharacter)
    {
        Vector3Int unitPosition = Utils.RoundXZFloorYInt(unitCharacter.transform.position);
        List<Vector3> pathVectorList = new List<Vector3>();

        int minSize = 1;
        int maxSize = 8;
        while (pathVectorList.Count == 0 && minSize <= maxSize)
        {
            List<Vector3Int> range = world.GetManhattas3DRange(unitPosition, minSize);
            range = SortTargetRangeByDistance(unitPosition, range);

            for (int i = 0; i < range.Count; i++)
            {
                if (IsTargetPositionExist(range[i])) { continue; }

                pathVectorList = (pathFinding.GetPathRoute(unitPosition, range[i]).pathVectorList);

                if (pathVectorList.Count != 0)
                {
                    teamPathRoutes.Add(new TeamPathRoute
                    {
                        unitCharacter = unitCharacter,
                        targetPosition = range[i],
                        pathRouteList = pathVectorList,
                        pathIndex = 0
                    });
                    return;
                }
            }
            minSize++;
        }

        if (pathVectorList.Count == 0)
        {
            pathVectorList.Add(unitPosition);
            teamPathRoutes.Add(new TeamPathRoute
            {
                unitCharacter = unitCharacter,
                targetPosition = unitPosition,
                pathRouteList = pathVectorList,
                pathIndex = 0
            });
        }
    }
}

