using System.Collections.Generic;
using UnityEngine;

public class AgressiveTargetRule : ScoreRuleBase
{
    public AgressiveTargetRule(List<IScoreSubRule> scoreSubRules, PathFinding pathFinding, int scoreBonus, bool debugMode) : base(scoreSubRules, pathFinding, scoreBonus, debugMode)
    {
    }

    public override float CalculateTargetScore(CharacterBase selfCharacter, 
        CharacterBase candidateCharacter)
    {
        if (candidateCharacter.currentTeam == selfCharacter.currentTeam)
        {
            if (debugMode)
                Debug.Log($"{selfCharacter} attempt to attack team member or ally {candidateCharacter}," +
                    $"Get Scorce Bonus: {scoreBonus}");
            return -100;
        }

        int selfMovementRange = selfCharacter.data.movementValue;

        int otherHealth = candidateCharacter.currentHealth;
        float ratio = 1 / otherHealth;


        return 0;
    }

    

}
