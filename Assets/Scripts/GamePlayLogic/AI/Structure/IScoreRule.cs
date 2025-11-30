using System.Collections.Generic;

public interface IScoreRule
{
    int ScoreBonus { get; }
    bool DebugMode { get; }
    int CalculateTargetScore(CharacterBase character);
    int CalculateMoveToTargetScore(CharacterBase character, 
        List<GameNode> targetAroundNodes, GameNode moveNode);
    int CalculateSkillScore(CharacterBase character, SkillData skill,
        GameNode moveNode, GameNode targetNode);
}
