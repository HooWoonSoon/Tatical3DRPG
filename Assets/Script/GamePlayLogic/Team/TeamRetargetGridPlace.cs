using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
public class TeamRetargetGridPlace : BaseGroupPathFinding
{
    public static TeamRetargetGridPlace instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    protected override void Start()
    {
        base.Start();
    }

    private List<Vector3Int> SortTargetRangeByDistance(Vector3Int from, List<Vector3Int> targets)
    {
        var sorted = new List<Vector3Int>(targets);
        sorted.Sort((a, b) => Vector3.Distance(from, a).CompareTo(Vector3.Distance(from, b)));
        return sorted;
    }

    public void EnterBattlePathFinding(List<PlayerCharacter> unitCharacters)
    {
        teamPathRoutes.Clear();
        for (int i = 0; i < unitCharacters.Count; i++)
        {
            if (unitCharacters[i].isBattle) { continue; }
            EnterBattlePathFinding(unitCharacters[i]);
            unitCharacters[i].isBattle = true;
            unitCharacters[i].pathRoute = teamPathRoutes[i];
            //unitCharacters[i].stateMachine.ChangeSubState(unitCharacters[i].movePathStateExplore);
        }
    }

    private void EnterBattlePathFinding(PlayerCharacter character)
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

                pathVectorList = (pathFinding.GetPathRoute(unitPosition, range[i]).pathRouteList);

                if (pathVectorList.Count != 0)
                {
                    teamPathRoutes.Add(new PathRoute
                    {
                        character = character,
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
            teamPathRoutes.Add(new PathRoute
            {
                character = character,
                targetPosition = unitPosition,
                pathRouteList = pathVectorList,
                pathIndex = 0
            });
        }
    }
}

