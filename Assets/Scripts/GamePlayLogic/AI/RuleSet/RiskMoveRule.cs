using System.Collections.Generic;
using Tactics.AI;
using UnityEngine;

public class RiskMoveRule : ScoreRuleBase
{
    public RiskMoveRule(DecisionSystem decisionSystem, List<IScoreRule> scoreSubRules, int scoreBonus, bool debugMode) : base(decisionSystem, scoreSubRules, scoreBonus, debugMode)
    {
    }

    public override float CalculateRiskMoveScore(CharacterBase character, 
        DecisionSystem.CharacterSkillInfluenceNodes characterSkillInfluenceNodes, GameNode moveNode)
    {
        float score = 0;

        int highestHealth = 0;
        foreach (var enemy in characterSkillInfluenceNodes.oppositeInfluence.Keys)
        {
            int health = enemy.data.health;
            if (health > highestHealth)
                highestHealth = health;
        }

        foreach (var enemy in characterSkillInfluenceNodes.oppositeInfluence.Keys)
        {
            float skillBestScore = 0;

            foreach (var skill in characterSkillInfluenceNodes.oppositeInfluence[enemy].Keys)
            {
                float skillScore = 0;

                var nodeList = characterSkillInfluenceNodes.oppositeInfluence[enemy][skill];

                GameNode currentNode = character.currentNode;
                currentNode.character = null;
                moveNode.character = character;

                if (!nodeList.Contains(moveNode)) continue;

                if (debugMode)
                    Debug.Log(
                        $"{enemy.data.characterName}, " +
                        $"Skill: {skill}, " +
                        $"Attackable: {moveNode.GetNodeVectorInt()}");

                if (scoreSubRules == null && scoreSubRules.Count == 0) continue;

                foreach (var subRule in scoreSubRules)
                {
                    skillScore += subRule.CalculateMoveSkillScore(enemy, skill, null, moveNode, highestHealth);
                }

                currentNode.character = character;
                moveNode.character = null;

                if (skillScore > skillBestScore)
                {
                    skillBestScore = skillScore;
                }
            }
            score -= skillBestScore;
        }

        foreach (var teammate in characterSkillInfluenceNodes.teammateInfluence.Keys)
        {
            float skillBestScore = 0;

            foreach (var skill in characterSkillInfluenceNodes.teammateInfluence[teammate].Keys)
            {
                float skillScore = 0;

                var nodeList = characterSkillInfluenceNodes.teammateInfluence[teammate][skill];

                GameNode currentNode = character.currentNode;
                currentNode.character = null;
                moveNode.character = character;

                if (!nodeList.Contains(moveNode)) continue;

                if (debugMode)
                    Debug.Log(
                        $"{teammate.data.characterName}, " +
                        $"Skill: {skill}, " +
                        $"Attackable: {moveNode.GetNodeVectorInt()}");

                if (scoreSubRules == null && scoreSubRules.Count == 0) continue;

                foreach (var subRule in scoreSubRules)
                {
                    score += subRule.CalculateMoveSkillScore(teammate, skill, null, moveNode, highestHealth);
                }

                currentNode.character = character;
                moveNode.character = null;

                if (skillScore > skillBestScore)
                {
                    skillBestScore = skillScore;
                }
            }
            score += skillBestScore;
        }

        if (debugMode)
            Debug.Log(
                $"<color=black>[MoveRule]</color> " +
                $"{character.data.characterName}, " +
                $"StartNode: {character.currentNode.GetNodeVectorInt()} " +
                $"MoveNode: {moveNode.GetNodeVectorInt()}, " +
                $"get Score bonus: {score}");

        return score;
    }
}
