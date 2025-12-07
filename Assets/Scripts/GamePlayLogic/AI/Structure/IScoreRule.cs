using System.Collections.Generic;
using Tactics.AI;

public interface IScoreRule
{
    int ScoreBonus { get; }
    bool DebugMode { get; }
    float CalculateTargetScore(CharacterBase selfCharacter, 
        CharacterBase targetCharacter);
    float CalculateMoveToTargetScore(CharacterBase character, 
        List<GameNode> targetAroundNodes, GameNode moveNode);
    float CalculateRiskMoveScore(CharacterBase character,
        DecisionSystem.CharacterSkillInfluenceNodes characterSkillInfluenceNodes,
        GameNode moveNode);
    float CalculateSkillScore(CharacterBase character, SkillData skill,
        GameNode moveNode, GameNode targetNode);
    float CalculateSkillScore(CharacterBase character, SkillData skill,
        GameNode moveNode, GameNode targetNode, int maxHealthAmongOpposites);
}
