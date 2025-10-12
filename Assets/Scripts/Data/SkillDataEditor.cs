using UnityEditor;
using UnityEngine;
using System;

[CustomEditor(typeof(SkillData))]
public class SkillDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SkillData data = (SkillData)target;

        data.skillName = EditorGUILayout.TextField("Skill Name", data.skillName);
        data.skillIcon = (Sprite)EditorGUILayout.ObjectField("Skill Icon", data.skillIcon, typeof(Sprite), false);
        data.description = EditorGUILayout.TextField("Description", data.description);

        Type[] allowedTypes = { Type.Skill, Type.Spell };
        int currentIndex = Array.IndexOf(allowedTypes, data.type);
        if (currentIndex < 0) currentIndex = 0;

        int newIndex = EditorGUILayout.Popup("Type", currentIndex, Array.ConvertAll(allowedTypes, t => t.ToString()));
        data.type = allowedTypes[newIndex];

        data.range = EditorGUILayout.IntField("Range", data.range);
        data.skillType = (SkillType)EditorGUILayout.EnumPopup("Skill Type", data.skillType);
        data.targetType = (SkillTargetType)EditorGUILayout.EnumPopup("Target Type", data.targetType);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Skill Details", EditorStyles.boldLabel);

        if (data.type == Type.Spell)
        {
            data.requiredSP = EditorGUILayout.IntField("Required SP", data.requiredSP);
        }

        if (data.skillType == SkillType.Acttack)
        {
            data.damageAmount = EditorGUILayout.IntField("Damage Amount", data.damageAmount);
        }
        else if (data.skillType == SkillType.Heal)
        {
            data.healAmount = EditorGUILayout.IntField("Heal Amount", data.healAmount);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Visual Preference", EditorStyles.boldLabel); 

        data.skillCastTime = EditorGUILayout.FloatField("Skill Cast Time", data.skillCastTime);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(data);
        }
    }
}

