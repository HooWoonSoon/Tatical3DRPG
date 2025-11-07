using UnityEditor;
[CustomEditor(typeof(CharacterData))]
public class CharacterDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CharacterData data = (CharacterData)target;
        CharacterEditorDrawer.DrawCharacterEditor(data);
    }
}