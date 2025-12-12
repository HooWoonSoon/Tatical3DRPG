using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public static class UtilityAIDrawer
{
    public enum Option
    {
        Rules, Parameters 
    }

    private static Option option;
    private static Vector2 scorllPos;

    public static void DrawUtilityCard(ref UtilityAIDatabase database, 
        ref UtilityAIScoreConfig selectedUtilityAI, UtilityAIScoreConfig data)
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();
        GUILayout.Label("Name: " + data.name, EditorStyles.boldLabel);
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        #region Layout
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Select", GUILayout.Width(60)))
            Selection.activeObject = data;
        if (GUILayout.Button("Edit", GUILayout.Width(50)))
        {
            selectedUtilityAI = data;
            GUI.FocusControl(null);
        }
        if (GUILayout.Button("Delete", GUILayout.Width(60)))
        {
            /// Confirm delete dialog, pop out warning
            if (EditorUtility.DisplayDialog("Delete Data", $"Are you sure you want to delete data: {data.name}?", "Yes", "No"))
            {
                database.RemoveData(data);
                string path = AssetDatabase.GetAssetPath(data);
                AssetDatabase.DeleteAsset(path);
                selectedUtilityAI = null;

                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        GUILayout.EndHorizontal();
        #endregion
        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
    public static void DrawAIScoringPanel(UtilityAIScoreConfig utilityAI)
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal("box");
        if (GUILayout.Button("Rules", EditorStyles.toolbarButton))
        {
            option = Option.Rules;
        }
        if (GUILayout.Button("Parameters", EditorStyles.toolbarButton))
        {
            option = Option.Parameters;
        }
        EditorGUILayout.EndHorizontal();

        scorllPos = EditorGUILayout.BeginScrollView(scorllPos);
        
        switch (option)
        { 
            case Option.Parameters:
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.LabelField("Utility AI Formula Parameters Configuration", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Harm Formula Parameters", EditorStyles.boldLabel);
                utilityAI.minHealthPriority =
                    EditorGUILayout.FloatField("Min Health Priority", utilityAI.minHealthPriority);
                utilityAI.minHarmPriority =
                    EditorGUILayout.FloatField("Min Harm Priority", utilityAI.minHarmPriority);
                utilityAI.priorityHealthFactor =
                    EditorGUILayout.FloatField("Priority Health Factor", utilityAI.priorityHealthFactor);
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Treat Formula Parameters", EditorStyles.boldLabel);
                utilityAI.minHealPriority =
                    EditorGUILayout.FloatField("Min Heal Priority", utilityAI.minHealPriority);
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Opposite Risk Formula Parameters", EditorStyles.boldLabel);
                utilityAI.riskInfluenceFactor =
                    EditorGUILayout.FloatField("Risk Influence Factor", utilityAI.riskInfluenceFactor);
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("General MP Formula Parameters", EditorStyles.boldLabel);
                utilityAI.mpMinReductionRatio =
                    EditorGUILayout.FloatField("MP Min Reduction Ratio", utilityAI.mpMinReductionRatio);
                utilityAI.mpMaxReductionRatio =
                    EditorGUILayout.FloatField("MP Max Reduction Ratio", utilityAI.mpMaxReductionRatio);
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
                break;
            case Option.Rules:
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.LabelField("Utility AI Scoring Rules Configuration", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical("box");

                DrawRuleGroup("MoveTargetRule", "Move Target Rule",
                    ref utilityAI.moveTargetRuleScore,
                    new SubRuleRef("Target Rule Score", () => utilityAI.targeRuleScore,
                    v => utilityAI.targeRuleScore = v));

                EditorGUILayout.Space(5);

                DrawRuleGroup("RiskMoveRule", "Risk Move Rule",
                    ref utilityAI.riskMoveRuleScore,
                    new SubRuleRef("Harm Rule Score", () => utilityAI.harmRuleScore,
                    v => utilityAI.harmRuleScore = v),
                    new SubRuleRef("Treat Rule Score", () => utilityAI.treatRuleScore,
                    v => utilityAI.treatRuleScore = v));

                EditorGUILayout.Space(5);

                DrawRuleGroup("HarmRule", "Origin Harm Rule",
                    ref utilityAI.originHarmRuleScore,
                    new SubRuleRef("Fatal Hit Rule Score", () => utilityAI.fatalHitRuleScore,
                    v => utilityAI.fatalHitRuleScore = v));

                EditorGUILayout.Space(5);

                DrawRuleGroup("TreatRule", "Origin Treat Rule",
                    ref utilityAI.originTreatRuleScore);

                EditorGUILayout.Space(5);

                DrawRuleGroup("RiskMoveHarmRule", "Risk Move Harm Rule",
                    ref utilityAI.riskMoveHarmRuleScore,
                    new SubRuleRef("Fatal Hit Rule Score", () => utilityAI.riskFatalHitRuleScore,
                    v => utilityAI.riskFatalHitRuleScore = v));

                EditorGUILayout.Space(5);

                DrawRuleGroup("RiskMoveTreatRule", "Risk Move Treat Rule",
                    ref utilityAI.riskMoveTreatRuleScore);

                EditorGUILayout.Space(5);

                DrawRuleGroup("DefenseBackRule", "Defense Back Rule",
                    ref utilityAI.defenseBackRuleRuleScore);

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndVertical();
                break;
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();
    }

    public class SubRuleRef
    {
        public string label;
        public Func<int> getter;
        public Action<int> setter;

        public SubRuleRef(string label, Func<int> getter, Action<int> setter)
        {
            this.label = label;
            this.getter = getter;
            this.setter = setter;
        }
    }

    private static void DrawRuleGroup(string className,
        string title,
        ref int headScore,
        params SubRuleRef[] subRules)
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"{title}", EditorStyles.boldLabel, GUILayout.Width(150));
        DrawScriptLink($"{className}");
        EditorGUILayout.EndHorizontal();

        headScore = EditorGUILayout.
            IntField($"{title} Score", headScore);

        if (subRules != null && subRules.Length > 0)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Sub Rules", EditorStyles.boldLabel);

            for (int i = 0; i < subRules.Length; i++)
            {
                var sub = subRules[i];
                int newValue = EditorGUILayout.IntField(sub.label, sub.getter());
                sub.setter(newValue);
            }

            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndVertical();
    }

    private static Dictionary<string, MonoScript> scriptCache = new Dictionary<string, MonoScript>();
    private static void DrawScriptLink(string className)
    {
        if (!scriptCache.TryGetValue(className, out MonoScript script))
        {
            string[] guids = AssetDatabase.FindAssets($"{className} t:Script");
            if (guids.Length == 0)
            {
                Debug.LogWarning($"Script not found: {className}");
                return;
            }
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            scriptCache[className] = script;
        }

        EditorGUILayout.ObjectField(script, typeof(MonoScript), false);
    }
}
