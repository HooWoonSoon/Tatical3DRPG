public class PrimaryTargetRule : ScoreRuleBase
{
    public PrimaryTargetRule(PathFinding pathFinding, int scoreBonus, bool debugMode) : base(pathFinding, scoreBonus, debugMode)
    {
    }

    public override int CalculateTargetScore(CharacterBase character)
    {
        return base.CalculateTargetScore(character);
    }
}
