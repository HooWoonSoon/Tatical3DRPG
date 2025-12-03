using System.Collections.Generic;

public interface IScoreRule
{
    int ScoreBonus { get; }
    bool DebugMode { get; }
    float CalculateTargetScore(CharacterBase selfCharacter, 
        CharacterBase candidateCharacter);
    float CalculateMoveToTargetScore(CharacterBase character, 
        List<GameNode> targetAroundNodes, GameNode moveNode);
    float CalculateMoveScore(CharacterBase character,
        List<GameNode> confiltNode, List<CharacterBase> opposites,
        List<GameNode> supportNode, List<CharacterBase> teammate,
        GameNode moveNode);
    float CalculateSkillScore(CharacterBase character, SkillData skill,
        GameNode moveNode, GameNode targetNode);
}
