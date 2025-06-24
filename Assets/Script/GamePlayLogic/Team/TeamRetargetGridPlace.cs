using System;
using System.Collections.Generic;
using UnityEngine;
public class TeamRetargetGridPlace : BaseGroupPathFinding
{
    private Action onReachTargetForBattle;

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
        AllUnitsToTarget();
        PathfindingMoveToTarget();
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
            if (unitCharacters[i].isBattle) { continue; }
            EnterBattlePathFinding(unitCharacters[i]);
            unitCharacters[i].isBattle = true;
        }
        isActivePathFinding = true;
    }

    private void EnterBattlePathFinding(Character character)
    {
        Vector3Int unitPosition = character.GetCharacterPosition();
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
                        unitCharacter = character,
                        targetPosition = range[i],
                        pathRouteList = pathVectorList,
                        pathIndex = 0
                    });
                    return ;
                }
            }
            minSize++;
        }

        if (pathVectorList.Count == 0)
        {
            pathVectorList.Add(unitPosition);
            teamPathRoutes.Add(new TeamPathRoute
            {
                unitCharacter = character,
                targetPosition = unitPosition,
                pathRouteList = pathVectorList,
                pathIndex = 0
            });
        }
    }
}

