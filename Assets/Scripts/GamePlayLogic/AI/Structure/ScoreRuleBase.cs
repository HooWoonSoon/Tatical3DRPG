public abstract class ScoreRuleBase : IScoreRule
{
    protected PathFinding pathFinding;
    protected int scoreBonus;
    protected bool debugMode;

    public PathFinding PathFinding => pathFinding;
    public int ScoreBonus => scoreBonus;
    public bool DebugMode => debugMode;

    protected ScoreRuleBase(PathFinding pathFinding, int scoreBonus, bool debugMode)
    {
        this.pathFinding = pathFinding;
        this.scoreBonus = scoreBonus;
        this.debugMode = debugMode;
    }

    public virtual int CalculateTargetScore(CharacterBase character) { return 0; }
    public virtual int CalculateMoveToTargetScore(CharacterBase character, 
        CharacterBase targetCharacter, GameNode moveNode) { return 0; }
    public virtual int CalculateSkillScore(CharacterBase character, SkillData skill,
        GameNode moveNode, GameNode targetNode) { return 0; }
}