using System.Collections.Generic;
using UnityEngine;
public class HarmRule : ScoreRuleBase
{
    public HarmRule(List<IScoreSubRule> scoreSubRules, PathFinding pathFinding, int scoreBonus, bool debugMode) : base(scoreSubRules, pathFinding, scoreBonus, debugMode)
    {
    }

    public override float CalculateSkillScore(CharacterBase character, SkillData skill, GameNode moveNode, GameNode targetNode)
    {
        if (skill == null) return 0;

        if (skill.skillType != SkillType.Acttack)
            return 0;

        CharacterBase target = targetNode.character;
        if (target == null) return 0;

        int damage = skill.damageAmount;
        int missingHealth = target.currentHealth;

        // Damage overflow
        int actualDamage = Mathf.Min(damage, missingHealth);

        if (actualDamage <= 0)
        {
            if (debugMode)
                Debug.Log($"Skill: {skill.skillName} damage skill," +
                    $" cannot deal damage to {target.data.characterName}, " +
                    $" Get Score bonus: -100");

            return -100;
        }

        float damageFactor = (float)actualDamage / target.data.health;
        float priorityFactor = Mathf.Max(0.2f, damageFactor);

        float mpCost = skill.MPAmount;
        float maxMp = Mathf.Max(1f, character.data.mental);
        float mpFactor = 1f - Mathf.Lerp(0, 0.4f, mpCost / maxMp);

        float t = priorityFactor * mpFactor;

        float score = Mathf.Lerp(0f, scoreBonus, t);

        foreach (var rule in scoreSubRules)
            score += rule.CalculateScore(character, skill, moveNode, targetNode);

        if (debugMode)
            Debug.Log(
                $"<color=red>[HarmRule]</color> " +
                $"{character.data.characterName}, " +
                $"<b>{skill.skillName}</b> damage skill, " +
                $"deal damage to {target.data.characterName}" +
                $"at Target Node {targetNode.GetNodeVectorInt()}, " + 
                $"plus Score bonus: {score}");

        return score;
    }
}
