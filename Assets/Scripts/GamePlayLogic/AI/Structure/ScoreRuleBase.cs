using System.Collections.Generic;

public abstract class ScoreRuleBase : IScoreRule
{
    protected List<IScoreSubRule> scoreSubRules;
    protected PathFinding pathFinding;
    protected int scoreBonus;
    protected bool debugMode;

    protected List<IScoreSubRule> ScoreSubRules => scoreSubRules;
    public PathFinding PathFinding => pathFinding;
    public int ScoreBonus => scoreBonus;
    public bool DebugMode => debugMode;

    protected ScoreRuleBase(List<IScoreSubRule> scoreSubRules, PathFinding pathFinding, int scoreBonus, bool debugMode)
    {
        this.scoreSubRules = scoreSubRules;
        this.pathFinding = pathFinding;
        this.scoreBonus = scoreBonus;
        this.debugMode = debugMode;
    }

    public virtual float CalculateTargetScore(CharacterBase character,
        CharacterBase candidateCharacter) { return 0; }
    public virtual float CalculateMoveToTargetScore(CharacterBase character, 
        List<GameNode> targetAroundNodes, GameNode moveNode) { return 0; }
    public virtual float CalculateMoveScore(CharacterBase character, 
        List<GameNode> confiltNode, List<CharacterBase> opposites, 
        List<GameNode> supportNode, List<CharacterBase> teammate, 
        GameNode moveNode) { return 0; }
    public virtual float CalculateSkillScore(CharacterBase character, SkillData skill,
        GameNode moveNode, GameNode targetNode) { return 0; }
}