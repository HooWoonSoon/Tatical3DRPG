using UnityEditor;

[CustomEditor(typeof(SkillData))]
public class SkillDataInpectorRedraw : Editor
{
    public override void OnInspectorGUI()
    {
        SkillData data = (SkillData)target;
        SkillEditorDrawer.DrawSkillEditor(data);
    }
}

