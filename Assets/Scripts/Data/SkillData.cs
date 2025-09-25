using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Tactics/Skill")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public int range;
    public int aoeRadius = 1; // If not aoe skill, set to 1
    public int baseDamage;

    public List<GameNode> GetInflueneNode(World world, GameNode origin)
    {
        List<GameNode> coverange = world.GetManhattas3DGameNode(origin.GetVectorInt(), range);
        return coverange;
    }
}

