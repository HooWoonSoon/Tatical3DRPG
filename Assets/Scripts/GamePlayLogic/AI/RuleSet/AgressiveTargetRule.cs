using System.Collections.Generic;
using UnityEngine;

public class AgressiveTargetRule : ScoreRuleBase
{
    public AgressiveTargetRule(List<IScoreRule> scoreSubRules, PathFinding pathFinding, int scoreBonus, bool debugMode) : base(scoreSubRules, pathFinding, scoreBonus, debugMode)
    {
    }

    public override float CalculateTargetScore(CharacterBase selfCharacter, 
        CharacterBase targetCharacter)
    {
        float score = 0;

        if (targetCharacter.currentTeam == selfCharacter.currentTeam)
        {
            if (debugMode)
                Debug.Log($"{selfCharacter} attempt to attack team member or ally {targetCharacter}," +
                    $"Get Scorce Bonus: {scoreBonus}");
            return -1;
        }
        
        if (targetCharacter.unitState == UnitState.Knockout
            || targetCharacter.unitState == UnitState.Dead)
        {
            return -1;
        }

        int otherCurrentHealth = targetCharacter.currentHealth;
        int otherHealth = targetCharacter.data.health;

        float t = (float)otherCurrentHealth / otherHealth;

        score = Mathf.Lerp(0, scoreBonus, t);

        return score;
    }
}
