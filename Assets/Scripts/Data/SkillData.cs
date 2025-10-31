using System.Collections.Generic;
using UnityEngine;
public enum SkillTargetType
{
    Self, Our, Opposite, Both
}
public enum SkillType
{
    Acttack, Heal
}

public enum Trajectory
{
    Linear, Parabola 
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
    public AbilityType type;
    public SkillType skillType;
    public SkillTargetType targetType;
    public bool skillDesignatedSelf;

    public bool isProjectile;
    public GameObject projectTilePrefab;
    [Range(0, 90)] public int initialElevationAngle;

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

