using System.Collections.Generic;
using Tactics.AI;

public interface IScoreRule
{
    int ScoreBonus { get; }
    bool DebugMode { get; }
    float CalculateTargetScore(CharacterBase selfCharacter, 
        CharacterBase targetCharacter, List<CharacterBase> teammates, 
        List<CharacterBase> opposites);
    float CalculateMoveToTargetScore(CharacterBase character, CharacterBase targetCharacter,
        List<GameNode> targetAroundNodes, GameNode moveNode, List<CharacterBase> teammates,
        List<CharacterBase> opposites, DecisionSystem.CharacterSkillInfluenceNodes characterSkillInfluenceNodes);
    float CalculateRiskMoveScore(CharacterBase character,
        DecisionSystem.CharacterSkillInfluenceNodes characterSkillInfluenceNodes,
        GameNode moveNode);
    float CalculateSkillScore(CharacterBase character, SkillData skill,
        GameNode targetNode, int maxHealthAmongOpposites);
    float CalculateMoveSkillScore(CharacterBase character, SkillData skill,
        GameNode moveNode, GameNode targetNode, int maxHealthAmongOpposites);
    float CalculateOrientationScore(CharacterBase character, GameNode originNode,
        Orientation orientation);
}
