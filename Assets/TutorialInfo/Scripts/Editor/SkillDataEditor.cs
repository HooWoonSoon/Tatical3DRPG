using UnityEditor;

[CustomEditor(typeof(SkillData))]
public class SkillDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SkillData data = (SkillData)target;
        SkillEditorDrawer.DrawSkillEditor(data);
    }
}

