using UnityEngine;
public class HarmRule : ScoreRuleBase
{
    public HarmRule(PathFinding pathFinding, int scoreBonus, bool debugMode) : base(pathFinding, scoreBonus, debugMode)
    {
    }

    public override int CalculateSkillScore(CharacterBase character, SkillData skill, GameNode moveNode, GameNode targetNode)
    {
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
                    $" get Score bonus: -100");

            return -100;
        }
        float damageRatioScore = (float)actualDamage / target.data.health * scoreBonus;

        float mpFactor;
        if (skill.MPAmount > 0)
            mpFactor = 1f / skill.MPAmount;
        else
            mpFactor = 1f;

        int score = Mathf.CeilToInt(damageRatioScore * mpFactor);

        if (debugMode)
            Debug.Log($"Skill: {skill.skillName} damage skill, " +
                $"deal damage to {target.data.characterName}, " + 
                $"plus Score bonus: {score}");

        return score;
    }
}
