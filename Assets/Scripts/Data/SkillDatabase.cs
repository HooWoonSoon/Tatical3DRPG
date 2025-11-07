using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillDatabase", menuName = "Tactics/Skill Database")]
public class SkillDatabase : ScriptableObject
{
    public List<SkillData> allSkills;

    private void OnValidate()
    {
        allSkills.RemoveAll(skill => skill == null);
    }

    public void AddSkill(SkillData skill)
    {
        allSkills.RemoveAll(skill => skill == null);
        if (!allSkills.Contains(skill)) 
            allSkills.Add(skill);
    }
}
