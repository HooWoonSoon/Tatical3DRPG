using UnityEditor;
[CustomEditor(typeof(CharacterData))]
public class CharacterDataInpectorRedraw : Editor
{
    public override void OnInspectorGUI()
    {
        CharacterData data = (CharacterData)target;
        CharacterEditorDrawer.DrawCharacterEditor(data);
    }
}