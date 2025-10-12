using System.Collections.Generic;
using UnityEngine;
public enum SkillTargetType
{
    Our, Opposite, Both
}
public enum SkillType
{
    Acttack, Heal
}

[CreateAssetMenu(fileName = "SkillData", menuName = "Tactics/Skill")]
public class SkillData : ScriptableObject
{
    [Header("Skill Info")]
    public string skillName;
    public Sprite skillIcon;
    public int range;
    public int aoeRadius = 1; // If not aoe skill, set to 1
    public string description;
    public Type type;
    public SkillType skillType;
    public SkillTargetType targetType;

    public Sprite spIcon;
    public int requiredSP;

    //  If skillType is Attack
    public int damageAmount;

    //  If skillType is Heal
    public int healAmount;

    public float skillCastTime = 1f;

    public List<GameNode> GetInflueneNode(World world, GameNode origin)
    {
        List<GameNode> coverange = world.GetManhattas3DGameNode(origin.GetVectorInt(), range);
        return coverange;
    }
}

