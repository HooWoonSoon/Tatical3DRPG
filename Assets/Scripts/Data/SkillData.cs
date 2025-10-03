using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Tactics/Skill")]
public class SkillData : ScriptableObject
{
    [Header("Skill Info")]
    public string skillName;
    public Sprite mainIcon;
    public int range;
    public int aoeRadius = 1; // If not aoe skill, set to 1
    public int baseDamage;

    public int requiredSP;
    public Sprite spIcon;

    [Header("Visual Preference")]
    [SerializeField] private float skillCastTime = 1f;

    #region external readonly
    public float SkillCastTime => skillCastTime;
    #endregion

    public List<GameNode> GetInflueneNode(World world, GameNode origin)
    {
        List<GameNode> coverange = world.GetManhattas3DGameNode(origin.GetVectorInt(), range);
        return coverange;
    }
}

