using System.Collections.Generic;
using UnityEngine;
public class UnitMoveRule : ScoreRuleBase
{
    public UnitMoveRule(List<IScoreSubRule> scoreSubRules, PathFinding pathFinding, int scoreBonus, bool debugMode) : base(scoreSubRules, pathFinding, scoreBonus, debugMode)
    {
    }

    public override float CalculateMoveToTargetScore(CharacterBase character, 
        List<GameNode> targetAroundNodes, GameNode moveNode)
    {
        CharacterData data = character.data;
        //  No Join the Rule
        if (data == null) return 0;
        if (targetAroundNodes == null || targetAroundNodes.Count == 0) return 0;
        if (moveNode == null) return 0;

        if (targetAroundNodes == null || targetAroundNodes.Count == 0)
            return 0;

        int bestCost = int.MaxValue;
        GameNode bestTargetNode = null;

        foreach (var targetNode in targetAroundNodes)
        {
            int cost = pathFinding.GetNodesBetweenCost(moveNode, 
                targetNode, character, 1, 1);

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
        float score = Mathf.Lerp(0f, scoreBonus, distanceFactor);

        if (debugMode)
            Debug.Log(
                $"<color=black>[UnitMoveRule]</color> " +
                $"{character.data.characterName}, " +
                $"StartNode: {character.currentNode.GetNodeVectorInt()} " +
                $"MoveNode: {moveNode.GetNodeVectorInt()}," +
                $"Route actual cost: {bestCost} " +
                $"TargetNode: {bestTargetNode.GetNodeVectorInt()}, " +
                $"get Score bonus: {score}");
        return score;
    }
}
