using System.Collections.Generic;
using UnityEngine;
public class HarmRule : ScoreRuleBase
{
    public HarmRule(List<IScoreRule> scoreSubRules, PathFinding pathFinding, int scoreBonus, bool debugMode) : base(scoreSubRules, pathFinding, scoreBonus, debugMode)
    {
    }

    public override float CalculateSkillScore(CharacterBase character, SkillData skill, 
        GameNode moveNode, GameNode targetNode, int maxHealthAmongOpposites)
    {
        if (skill == null) 
            return 0;
        if (skill.skillType != SkillType.Acttack)
            return 0;
        if (skill.MPAmount > character.currentMental) return 0;

        CharacterBase target = targetNode.character;
        if (target == null) return 0;

        int damage = skill.damageAmount;
        int targetReleaseHealth = target.currentHealth;

        // Damage overflow
        int actualDamage = Mathf.Min(damage, targetReleaseHealth);

        if (actualDamage <= 0)
        {
            if (debugMode)
                Debug.Log($"Skill: {skill.skillName} damage skill," +
                    $" cannot deal damage to {target.data.characterName}, " +
                    $" Get Score bonus: Min Value");
            return 0;
        }

        float healthFactor = 1f - ((float)target.currentHealth / maxHealthAmongOpposites);
        float priorityHealthFactor = Mathf.Max(0.2f, healthFactor);

        float damageFactor = (float)actualDamage / target.data.health;
        float priorityDamageFactor = Mathf.Max(0.2f, damageFactor);

        float mpCost = skill.MPAmount;
        float maxMp = Mathf.Max(1f, character.data.mental);
        float mpFactor = 1f - Mathf.Lerp(0, 0.4f, mpCost / maxMp);

        float t = priorityDamageFactor * mpFactor + priorityHealthFactor * 0.3f;

        float score = Mathf.Lerp(0f, scoreBonus, t);

        if (scoreSubRules != null && scoreSubRules.Count > 0)
        {
            foreach (var subRule in scoreSubRules)
                score += subRule.CalculateSkillScore(character, skill, moveNode, targetNode);
        }

        if (score > scoreBonus)
            score = scoreBonus;

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
