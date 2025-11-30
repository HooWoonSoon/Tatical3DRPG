using System.Collections.Generic;
using UnityEngine;
public class UnitMoveRule : ScoreRuleBase
{
    public UnitMoveRule(PathFinding pathFinding, int scoreBonus, bool debugMode) : base(pathFinding, scoreBonus, debugMode)
    {
    }

    public override int CalculateMoveToTargetScore(CharacterBase character, 
        List<GameNode> targetAroundNodes, GameNode moveNode)
    {
        CharacterData data = character.data;
        //  No Join the Rule
        if (data == null) return 0;
        if (targetAroundNodes == null || targetAroundNodes.Count == 0) return 0;
        if (moveNode == null) return 0;

        if (debugMode)
            Debug.Log("Execute Unit Move Rule");

        if (targetAroundNodes == null || targetAroundNodes.Count == 0)
            return 0;

        int bestCost = int.MaxValue;
        GameNode bestTargetNode = null;

        foreach (var targetNode in targetAroundNodes)
        {
            int cost = pathFinding.GetNodesBetweenCost(moveNode, targetNode, 2, 0); // heightCheck=2, riseLimit=0示例
            if (cost < bestCost)
            {
                bestCost = cost;
                bestTargetNode = targetNode;
            }
        }

        if (bestCost == int.MaxValue)
        {
            if (debugMode)
                Debug.Log("No valid path to any target node");
            return -100; // Dead Path
        }

        float distanceFactor = 1f / (1f + bestCost);        
        int score = Mathf.RoundToInt(Mathf.Lerp(0f, scoreBonus, distanceFactor));

        Debug.Log($"{character.data.characterName}, MoveNode: {moveNode.GetNodeVectorInt()}, " +
            $"TargetNode: {bestTargetNode.GetNodeVectorInt()}, get Score bonus: {score}");
        return score;
    }
}
