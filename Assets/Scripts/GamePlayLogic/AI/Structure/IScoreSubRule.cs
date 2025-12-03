using System.Collections.Generic;

public interface IScoreSubRule
{
    int ScoreBonus { get; }
    bool DebugMode { get; }
    float CalculateScore(CharacterBase character, SkillData skill,
        GameNode moveNode, GameNode targetNode);
}
