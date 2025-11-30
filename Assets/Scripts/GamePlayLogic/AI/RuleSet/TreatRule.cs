using UnityEngine;
public class TreatRule : ScoreRuleBase
{
    public TreatRule(PathFinding pathFinding, int scoreBonus, bool debugMode) : base(pathFinding, scoreBonus, debugMode)
    {
    }

    public override int CalculateSkillScore(CharacterBase character, SkillData skill, GameNode moveNode, GameNode targetNode)
    {
        //  No Join the Rule
        if (skill == null) 
            return 0;
        if (skill.skillType != SkillType.Heal)
            return 0;

        CharacterBase target = targetNode.character;

        if (debugMode)
        {
            Debug.Log("Execute Treat Rule");
            if (target != null)
                Debug.Log($"{character.data.characterName} consider to heal " +
                    $"{target.data.characterName} with skill: {skill.skillName}");
        }

        int missingHealth = target.data.health - target.currentHealth;
        float targetHealthRatio = (float)target.currentHealth / target.data.health;

        if (missingHealth <= 0)
            return -int.MaxValue;

        int actualHeal = Mathf.Min(skill.healAmount, missingHealth);

        float healFactor = (float)actualHeal / target.data.health;
        float missingFactor = (float)missingHealth / target.data.health;
        float priorityFactor = missingFactor * missingFactor;

        float maxMp = Mathf.Max(1f, character.data.mental);
        float mpFactor = 1f - Mathf.Clamp01(skill.MPAmount / maxMp);

        float t = healFactor * priorityFactor * mpFactor;

        int score = Mathf.RoundToInt(Mathf.Lerp(0f, scoreBonus, t));

        if (debugMode)
            Debug.Log($"Skill: {skill.skillName} heal skill, " +
              $"Get Score bonus: {score}");

        return score;
    }
}

