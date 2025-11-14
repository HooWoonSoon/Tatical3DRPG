using UnityEditor;
using UnityEngine;
using System;

public static class SkillEditorDrawer
{
    public static void DrawSkillEditor(SkillData data)
    {
        data.skillName = EditorGUILayout.TextField("Skill Name", data.skillName);
        data.skillIcon = (Sprite)EditorGUILayout.ObjectField("Skill Icon", data.skillIcon, typeof(Sprite), false);
        data.description = EditorGUILayout.TextField("Description", data.description);

        AbilityType[] allowedTypes = { AbilityType.Skill, AbilityType.Spell };
        int currentIndex = Array.IndexOf(allowedTypes, data.type);
        if (currentIndex < 0) currentIndex = 0;

        int newIndex = EditorGUILayout.Popup("Type", currentIndex, Array.ConvertAll(allowedTypes, t => t.ToString()));
        data.type = allowedTypes[newIndex];

        data.skillType = (SkillType)EditorGUILayout.EnumPopup("Skill Type", data.skillType);
        data.targetType = (SkillTargetType)EditorGUILayout.EnumPopup("Target Type", data.targetType);
        data.skillRange = EditorGUILayout.IntField("Skill Range", data.skillRange);
        data.occlusionRange = EditorGUILayout.IntField("Range Occulsion From Center", data.occlusionRange);
        if (data.occlusionRange > data.skillRange)
        {
            data.occlusionRange = data.skillRange;
            Debug.LogWarning("Occlussion range are not allow to execess the skill range");
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Skill Details", EditorStyles.boldLabel);

        if (data.type == AbilityType.Skill)
        {
            data.requireMP = EditorGUILayout.Toggle("Require MP", data.requireMP);
            if (data.requireMP)
                data.MPAmount = EditorGUILayout.IntField("MP Amount", data.MPAmount);
        }
        else if (data.type == AbilityType.Spell)
        {
            data.requireMP = true;
            data.MPAmount = EditorGUILayout.IntField("MP Amount", data.MPAmount);
        }

        if (data.skillType == SkillType.Acttack)
        {
            DrawProjectileSection(data);
            data.damageAmount = EditorGUILayout.IntField("Damage Amount", data.damageAmount);
        }
        else if (data.skillType == SkillType.Heal)
        {
            DrawProjectileSection(data);
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

    private static void DrawProjectileSection(SkillData data)
    {
        if (data.targetType == SkillTargetType.Self)
        {
            data.skillRange = 1;
        }
        else if (data.targetType == SkillTargetType.Our ||
            data.targetType == SkillTargetType.Both ||
            data.targetType == SkillTargetType.Opposite)
        {
            data.isProjectile = EditorGUILayout.Toggle("Is Projectile", data.isProjectile);
            if (data.isProjectile)
            {
                data.projectTilePrefab = (GameObject)EditorGUILayout.ObjectField("Projectile Prefab", data.projectTilePrefab, typeof(GameObject), false);
                data.initialElevationAngle = EditorGUILayout.IntSlider("Initial Elevation Angle", data.initialElevationAngle, 0, 90);
            }
        }
    }
}
