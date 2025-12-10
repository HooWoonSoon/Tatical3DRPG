using System.Collections.Generic;
using Tactics.AI;
using UnityEngine;
public class MoveTreatRule : ScoreRuleBase
{
    public MoveTreatRule(DecisionSystem decisionSystem, List<IScoreRule> scoreSubRules, int scoreBonus, bool debugMode) : base(decisionSystem, scoreSubRules, scoreBonus, debugMode)
    {
    }

    public override float CalculateMoveSkillScore(CharacterBase character, SkillData skill, 
        GameNode moveNode, GameNode targetNode, int maxHealthAmongOpposites)
    {
        //  No Join the Rule
        if (skill == null) return 0;
        if (skill.skillType != SkillType.Heal)return 0;
        if (skill.MPAmount > character.currentMental) return 0;

        CharacterBase target = targetNode.character;

        int missingHealth = target.data.health - target.currentHealth;
        if (missingHealth <= 0) return 0;

        int actualHeal = Mathf.Min(skill.healAmount, missingHealth);
        if (actualHeal == 0) return 0;

        float healFactor = (float)actualHeal / skill.healAmount;
        float missingFactor = (float)missingHealth / target.data.health;
        float priorityFactor = Mathf.Max(0.19f, missingFactor * healFactor);

        float mpCost = skill.MPAmount;
        float maxMp = Mathf.Max(1f, character.data.mental);
        float mpFactor = 1f - Mathf.Lerp(0, 0.3f, mpCost / maxMp);

        float t = priorityFactor * mpFactor;

        float score = Mathf.Lerp(0f, scoreBonus, t);

        if (debugMode)
            Debug.Log(
                $"<color=#00BFFF>[MoveTreatRule]</color> " +
                $"{character.data.characterName}, " +
                $"<b>{skill.skillName}</b> heal skill, " +
                $"deal heal to {target.data.characterName}," +
                $"at Target Node {targetNode.GetNodeVectorInt()} " +
                $"plus Score bonus: {score}");

        return score;
    }
}

