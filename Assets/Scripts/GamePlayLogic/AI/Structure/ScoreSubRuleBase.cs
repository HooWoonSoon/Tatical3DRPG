using System.Collections.Generic;

public abstract class ScoreSubRuleBase : IScoreSubRule
{
    protected int scoreBonus;
    protected bool debugMode;

    public int ScoreBonus => scoreBonus;
    public bool DebugMode => debugMode;

    protected ScoreSubRuleBase(int scoreBonus, bool debugMode)
    {
        this.scoreBonus = scoreBonus;
        this.debugMode = debugMode;
    }

    public virtual float CalculateScore(CharacterBase character, SkillData skill,
        GameNode moveNode, GameNode targetNode)
    { return 0; }
}