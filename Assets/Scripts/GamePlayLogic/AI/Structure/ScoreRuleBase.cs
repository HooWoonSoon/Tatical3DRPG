using System.Collections.Generic;
using static Tactics.AI.DecisionSystem;

public abstract class ScoreRuleBase : IScoreRule
{
    protected List<IScoreRule> scoreSubRules;
    protected PathFinding pathFinding;
    protected int scoreBonus;
    protected bool debugMode;

    protected List<IScoreRule> ScoreSubRules => scoreSubRules;
    public PathFinding PathFinding => pathFinding;
    public int ScoreBonus => scoreBonus;
    public bool DebugMode => debugMode;

    protected ScoreRuleBase(List<IScoreRule> scoreSubRules, PathFinding pathFinding, int scoreBonus, bool debugMode)
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
    public virtual float CalculateRiskMoveScore(CharacterBase character,
        CharacterSkillInfluenceNodes characterSkillInfluenceNodes,
        GameNode moveNode) { return 0; }
    public virtual float CalculateSkillScore(CharacterBase character, SkillData skill,
        GameNode moveNode, GameNode targetNode) { return 0; }
    public virtual float CalculateSkillScore(CharacterBase character, SkillData skill, 
        GameNode moveNode, GameNode targetNode, int maxHealthAmongOpposites)
    { return 0; }
}