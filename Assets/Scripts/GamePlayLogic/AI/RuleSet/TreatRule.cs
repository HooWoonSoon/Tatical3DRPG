using System.Collections.Generic;
using UnityEngine;
public class TreatRule : ScoreRuleBase
{
    public TreatRule(List<IScoreRule> scoreSubRules, PathFinding pathFinding, int scoreBonus, bool debugMode) : base(scoreSubRules, pathFinding, scoreBonus, debugMode)
    {
    }

    public override float CalculateSkillScore(CharacterBase character, SkillData skill, GameNode moveNode, GameNode targetNode)
    {
        //  No Join the Rule
        if (skill == null) 
            return 0;
        if (skill.skillType != SkillType.Heal)
            return 0;
        if (skill.MPAmount > character.currentMental) return 0;

        CharacterBase target = targetNode.character;

        int missingHealth = target.data.health - target.currentHealth;
        float targetHealthRatio = (float)target.currentHealth / target.data.health;

        if (missingHealth <= 0)
            return 0;

        int actualHeal = Mathf.Min(skill.healAmount, missingHealth);

        float healFactor = (float)actualHeal / skill.healAmount;
        float missingFactor = (float)missingHealth / target.data.health;
        float priorityFactor = Mathf.Max(0.19f, missingFactor * healFactor);

        float mpCost = skill.MPAmount;
        float maxMp = Mathf.Max(1f, character.data.mental);
        float mpFactor = 1f - Mathf.Lerp(0, 0.4f, mpCost / maxMp);

        float t = priorityFactor * mpFactor;

        float score = Mathf.Lerp(0f, scoreBonus, t);

        if (debugMode)
            Debug.Log(
                $"<color=#00BFFF>[TreatRule]</color> " +
                $"{character.data.characterName}, " +
                $"<b>{skill.skillName}</b> heal skill, " +
                $"deal heal to {target.data.characterName}," +
                $"at Target Node {targetNode.GetNodeVectorInt()} " +
                $"plus Score bonus: {score}");

        return score;
    }
}

