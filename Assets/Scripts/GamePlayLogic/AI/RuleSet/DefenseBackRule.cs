using System.Collections.Generic;
using Tactics.AI;
using UnityEngine;

public class DefenseBackRule : ScoreRuleBase
{
    public DefenseBackRule(DecisionSystem decisionSystem, UtilityAIScoreConfig utilityAI, List<IScoreRule> scoreSubRules, int scoreBonus, RuleDebugContext context) : base(decisionSystem, utilityAI, scoreSubRules, scoreBonus, context)
    {
    }

    protected override bool DebugMode => DebugManager.IsDebugEnabled(context);

    public override float CalculateOrientationScore(CharacterBase character, 
        GameNode originNode, Orientation orientation)
    {
        float score = 0;

        PathFinding pathFinding = decisionSystem.pathFinding;
        List<Vector3Int> neighbourPosList = pathFinding.GetNeighbourPosCustomized(originNode.GetNodeVectorInt(), 1);
        string neighbourNodesMsg = string.Join(", ", neighbourPosList);
        Vector3Int orientationDir = character.GetOrientationDirection(orientation);
        World world = pathFinding.world;

        foreach (Vector3Int neighbourPos in neighbourPosList)
        {
            GameNode neighbourNode = world.GetNode(neighbourPos);

            if (neighbourNode == null || neighbourNode != null && !neighbourNode.isWalkable)
            {
                Vector3 originPos = originNode.GetNodeVector();

                Vector3 direction = (originPos - neighbourPos).normalized;
                Vector3Int nodeOrientationDir = character.GetDirConvertOrientationDir(direction);
                if (orientationDir == nodeOrientationDir)
                {
                    score += 1;
                }
            }
        }

        if (DebugMode)
            Debug.Log(
                $"<color=purple>[DefenseBackRule]</color> " +
                $"{character.data.characterName}, " +
                $"Origin Node: {originNode.GetNodeVectorInt()}, " +
                $"Neighbour Nodes: {neighbourNodesMsg}, " +
                $"to orientation {orientation}, " +
                $"plus Score bonus: {score}");

        return score;
    }
}
