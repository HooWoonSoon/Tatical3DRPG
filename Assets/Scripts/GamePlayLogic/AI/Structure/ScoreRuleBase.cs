using System.Collections.Generic;
using Tactics.AI;

public abstract class ScoreRuleBase : IScoreRule
{
    protected DecisionSystem decisionSystem;
    protected UtilityAIScoreConfig utilityAI;
    protected List<IScoreRule> scoreSubRules;
    protected int scoreBonus;
    protected bool debugMode;

    protected DecisionSystem DecisionSystem => decisionSystem;
    protected UtilityAIScoreConfig UtilityAI => utilityAI;
    protected List<IScoreRule> ScoreSubRules => scoreSubRules;
    public int ScoreBonus => scoreBonus;
    public bool DebugMode => debugMode;

    protected ScoreRuleBase(DecisionSystem decisionSystem, UtilityAIScoreConfig utilityAI, List<IScoreRule> scoreSubRules, int scoreBonus, bool debugMode)
    {
        this.decisionSystem = decisionSystem;
        this.utilityAI = utilityAI;
        this.scoreSubRules = scoreSubRules;
        this.scoreBonus = scoreBonus;
        this.debugMode = debugMode;
    }

    public virtual float CalculateTargetScore(CharacterBase character,
        CharacterBase candidateCharacter, List<CharacterBase> teammates, 
        List<CharacterBase> opposites) { return 0; }
    public virtual float CalculateMoveToTargetScore(CharacterBase character, float frontLineIndex,
        CharacterBase targetCharacter, List<GameNode> targetAroundNodes, 
        GameNode moveNode, List<CharacterBase> teammates, List<CharacterBase> opposites,
        DecisionSystem.CharacterSkillInfluenceNodes characterSkillInfluenceNodes) 
    { return 0; }
    public virtual float CalculateRiskMoveScore(CharacterBase character,
        DecisionSystem.CharacterSkillInfluenceNodes characterSkillInfluenceNodes,
        GameNode moveNode) { return 0; }
    public virtual float CalculateSkillScore(CharacterBase character, SkillData skill,
        GameNode targetNode, int highestHealthAmongCharacters) { return 0; }
    public virtual float CalculateMoveSkillScore(CharacterBase character, SkillData skill, 
        GameNode moveNode, GameNode targetNode, int highestHealthAmongCharacters)
    { return 0; }
    public virtual float CalculateRiskMoveSkillScore(CharacterBase character, SkillData skill,
        DecisionSystem.CharacterSkillInfluenceNodes characterSkillInfluenceNodes,
        GameNode moveNode, GameNode targetNode, int highestHealthAmongCharacters)
    { return 0; }
    public virtual float CalculateOrientationScore(CharacterBase character, 
        GameNode originNode, Orientation orientation) 
    { return 0; }
}