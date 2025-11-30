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

        int score = 0;

        if (targetHealthRatio <= 0.25f)
        {
            if (debugMode)
                Debug.Log($"Skill: {skill.skillName} critical heal skill, " +
                    $"low HP plus Score bonus: {scoreBonus}");
            return scoreBonus;
        }
        else if (targetHealthRatio <= 0.5f)
        {
            score = Mathf.CeilToInt(scoreBonus * Mathf.Lerp(0.7f, 1f, targetHealthRatio));

            if (debugMode)
                Debug.Log($"Skill: {skill.skillName} heal skill, " +
                    $"medial HP plus Score bonus: {score}");
            return score;
        }
        else if (targetHealthRatio <= 0.75f)
        {
            score = Mathf.CeilToInt(scoreBonus * Mathf.Lerp(0.5f, 0.7f, targetHealthRatio));
            
            if (debugMode)
                Debug.Log($"Skill: {skill.skillName} heal skill, " +
                    $"high HP plus Score bonus: {score}");
            return score;
        }
        else if (targetHealthRatio <= 1.0f)
        {
            int predictHeal = skill.healAmount;
            int actualHeal = Mathf.Min(predictHeal, missingHealth);

            float healRatio = (float)actualHeal / predictHeal;
            float tempoScore = Mathf.CeilToInt(scoreBonus * Mathf.Lerp(0f, 0.5f, targetHealthRatio));

            if (healRatio == 0)
            {
                Debug.Log($"Skill: {skill.skillName} heal skill, " +
                    $"full HP overflow get Score bonus: -100");
                return -100;
            }
            if (healRatio > 0)
                score = Mathf.CeilToInt(tempoScore * healRatio);

            float mpFactor;
            if (skill.MPAmount > 0)
                mpFactor = 1f / skill.MPAmount;
            else
                mpFactor = 1f;

            score = Mathf.CeilToInt(score * mpFactor);

            if (debugMode)
                Debug.Log($"Skill: {skill.skillName} heal skill, " +
                    $"almost full HP plus Score bonus: {score}");

            return score;
        }
        return 0;
    }
}

