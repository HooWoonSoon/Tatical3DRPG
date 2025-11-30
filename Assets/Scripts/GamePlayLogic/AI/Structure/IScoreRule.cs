public interface IScoreRule
{
    int ScoreBonus { get; }
    bool DebugMode { get; }
    int CalculateTargetScore(CharacterBase character);
    int CalculateMoveToTargetScore(CharacterBase character, 
        CharacterBase targetCharacter, GameNode moveNode);
    int CalculateSkillScore(CharacterBase character, SkillData skill,
        GameNode moveNode, GameNode targetNode);
}
