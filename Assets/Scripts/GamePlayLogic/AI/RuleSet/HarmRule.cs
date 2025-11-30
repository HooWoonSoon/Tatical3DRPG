using UnityEngine;
using static UnityEditor.PlayerSettings;
public class HarmRule : ScoreRuleBase
{
    public HarmRule(PathFinding pathFinding, int scoreBonus, bool debugMode) : base(pathFinding, scoreBonus, debugMode)
    {
    }

    public override int CalculateSkillScore(CharacterBase character, SkillData skill, GameNode moveNode, GameNode targetNode)
    {
        if (skill == null) return 0;

        if (skill.skillType != SkillType.Acttack)
            return 0;

        CharacterBase target = targetNode.character;
        if (target == null) return 0;

        if (debugMode)
        {
            Debug.Log("Execute Harm Rule");
            Debug.Log($"{character.data.characterName} consider to use " +
                $"{skill.skillName} damage on {target.data.characterName}");
        }

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

        float mpCost = skill.MPAmount;
        float maxMp = Mathf.Max(1f, character.data.mental);
        float mpFactor = 1f - Mathf.Clamp01(mpCost / maxMp);

        float t = damageFactor * mpFactor;

        int score = Mathf.RoundToInt(Mathf.Lerp(0f, scoreBonus, t));

        if (debugMode)
            Debug.Log($"Skill: {skill.skillName} damage skill, " +
                $"deal damage to {target.data.characterName}, " + 
                $"plus Score bonus: {score}");

        return score;
    }
}
