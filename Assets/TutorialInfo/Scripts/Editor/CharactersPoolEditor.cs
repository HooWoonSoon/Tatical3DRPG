using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.TextCore.Text;
public class CharactersPoolEditor : EditorWindow
{
    private enum PanelState
    {
        CharacterPool,
        CharacterData
    }

    private Texture2D bannerImage;
    private string searchFilter = "";
    private Vector2 scrollPos;
    private Vector2 scrollPos2;

    private CharacterPools manager;
    private CharacterBase selectedCharacter;

    private CharacterDatabase database;
    private CharacterData selectedCharacterData;
    private string updateName = "";

    private bool enabledCharacterConfiguration = false;

    private PanelState currentPanel = PanelState.CharacterPool;


    [MenuItem("Tactics/Characters Pool Editor")]
    private static void ShowWindow()
    {
        CharactersPoolEditor window = GetWindow<CharactersPoolEditor>();
        window.minSize = new Vector2(800, 700);
    }

    private void OnEnable()
    {
        LoadBannerImage();
        LoadCharacterPoolManager();
        LoadCharacterDatabase();
    }

    private void LoadBannerImage()
    {
        bannerImage = AssetDatabase.LoadAssetAtPath<Texture2D>
            ("Assets/EditorAssets/Banner_CharactersPoolEditor.jpg");
        if (bannerImage == null)
            Debug.LogWarning("Banner image not found. Make sure it's in EditorAssets folder.");
    }

    private void LoadCharacterPoolManager()
    {
        manager = GameObject.Find("Characters Pool").GetComponent<CharacterPools>();
        if (manager == null)
            Debug.LogWarning("Character pool not found. Make sure it's in Inpectors");
    }

    private void LoadCharacterDatabase()
    {
        string[] guids = AssetDatabase.FindAssets("t:CharacterDatabase");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path != null)
            {
                database = AssetDatabase.LoadAssetAtPath<CharacterDatabase>(path);
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.Space(5);
        GUIStyle tileGUI = new GUIStyle(EditorStyles.boldLabel);
        tileGUI.alignment = TextAnchor.MiddleCenter;
        tileGUI.fontSize = 26;
        EditorGUILayout.LabelField("Character Pool Editor", tileGUI);
        GUILayout.Space(5);

        if (bannerImage != null)
        {
            float bannerHeight = 100;
            float bannerWidth = position.width - 10;

            Rect bannerRect = new Rect(5, 10 + tileGUI.fontSize, bannerWidth, bannerHeight);

            GUI.DrawTexture(bannerRect, bannerImage, ScaleMode.ScaleAndCrop);

            EditorGUILayout.Space(bannerHeight + 10f);
        }

        #region UIRegion 1
        GUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Option Mode Interface", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Character Pool"))
        {
            currentPanel = PanelState.CharacterPool;
        }
        if (GUILayout.Button("Character Data"))
        {
            currentPanel = PanelState.CharacterData;
        }
        GUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        GUILayout.EndHorizontal();
        #endregion

        searchFilter = EditorGUILayout.TextField(searchFilter, EditorStyles.toolbarSearchField);

        switch (currentPanel)
        {
            case PanelState.CharacterPool:
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Refresh", GUILayout.Width(80), GUILayout.Height(25)))
                {
                    LoadBannerImage();
                    LoadCharacterPoolManager();
                    LoadCharacterDatabase();
                }
                if (GUILayout.Button("Create Characters Pool", GUILayout.Width(160), GUILayout.Height(25)))
                {
                    CreateCharacterPool();
                }
                if (GUILayout.Button("Select Character Pool", GUILayout.Width(160), GUILayout.Height(25)))
                {
                    if (manager != null) Selection.activeObject = manager;
                }
                GUILayout.EndHorizontal();

                #region UIRegion Character Pool
                GUILayout.BeginHorizontal();

                #region UIRegion Character Pool - 1
                DrawCharacterListPanel();
                #endregion

                GUILayout.BeginVertical("box");
                scrollPos2 = EditorGUILayout.BeginScrollView(scrollPos2);
                GUILayout.Label("Editor", EditorStyles.boldLabel);

                if (enabledCharacterConfiguration)
                {
                    CharacterConfigurationDrawer.DrawCharacterConfiguration(manager);
                }
                else
                {
                    if (selectedCharacter != null)
                    {
                        CharacterEditorDrawer.DrawCharacterEditor(selectedCharacter);
                        if (selectedCharacter.data != null)
                            CharacterEditorDrawer.DrawCharacterEditor(selectedCharacter.data);
                        else
                            EditorGUILayout.HelpBox("Missing character data", MessageType.Info);
                    }
                    else
                        EditorGUILayout.HelpBox("Press Edit button to select character to edit", MessageType.Info);
                }
                GUILayout.EndScrollView();

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                #endregion
                break;
            case PanelState.CharacterData:
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Create Character Database", GUILayout.Width(180), GUILayout.Height(25)))
                {

                }
                if (GUILayout.Button("Select Character Database", GUILayout.Width(180), GUILayout.Height(25)))
                {
                    if (database != null) Selection.activeObject = database;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                GUILayout.BeginVertical("box", GUILayout.Width(215));
                GUILayout.Label("Data List", EditorStyles.boldLabel);

                if (GUILayout.Button("Create New Character Data", EditorStyles.toolbarButton)) CreateData();
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

                if (database != null && database.allCharacterDatas != null)
                {
                    var validList = database.allCharacterDatas.Where(d => d != null && 
                    (string.IsNullOrEmpty(searchFilter)) || d.name.ToLower().Contains(searchFilter.ToLower())).
                    ToList();

                    if (validList.Count == 0)
                    {
                        EditorGUILayout.HelpBox("Character datas is empty. Please create more data.", MessageType.Info);
                    }
                    else
                    {
                        foreach (var data in validList)
                        {
                            DrawDataCard(data);
                        }
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No character data loaded or empty.", MessageType.Warning);
                }
                GUILayout.EndScrollView();
                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");

                GUILayout.BeginHorizontal();
                GUILayout.Label("Scriptable Name", EditorStyles.boldLabel);
                updateName = EditorGUILayout.TextField(updateName);

                if (GUILayout.Button("Update Name"))
                {
                    AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(selectedCharacterData), updateName);
                }
                GUILayout.EndHorizontal();
                EditorGUILayout.Space();

                GUILayout.Label("Editor", EditorStyles.boldLabel);

                if (selectedCharacterData != null)
                {
                    CharacterEditorDrawer.DrawCharacterEditor(selectedCharacterData);
                }
                else
                    EditorGUILayout.HelpBox("Press Edit button to select data to edit", MessageType.Info);

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                break;
        }
    }

    private void DrawCharacterListPanel()
    {
        GUILayout.BeginVertical("box", GUILayout.Width(215));
        GUILayout.Label("Character List", EditorStyles.boldLabel);
        if (GUILayout.Button("New Character Configuration", EditorStyles.toolbarButton))
        {
            enabledCharacterConfiguration = !enabledCharacterConfiguration;
        }
        scrollPos = GUILayout.BeginScrollView(scrollPos);

        if (manager == null)
        {
            EditorGUILayout.HelpBox("Character pool manager is missing. Please create one.", MessageType.Info);
            selectedCharacter = null;
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            return;
        }

        List<CharacterBase> allCharacterList = manager.allCharacter.ToList();
        List<CharacterBase> list = allCharacterList.Where
            (c => c != null && (string.IsNullOrEmpty(searchFilter)) ||
            c.name.ToLower().Contains(searchFilter.ToLower())).ToList();

        if (list.Count == 0)
        {
            EditorGUILayout.HelpBox("Character Pool is empty. Please create more character", MessageType.Info);
        }
        else
        {
            for (int i = 0; i < list.Count; i++)
            {
                DrawCharacterCard(list[i]);
            }
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void CreateCharacterPool()
    {
        GameObject poolGameObject = GameObject.Find("Characters Pool");
        if (poolGameObject == null)
            manager = new GameObject("Characters Pool").AddComponent<CharacterPools>();
        else
        {
            CharacterPools characterPoolsManager = poolGameObject.GetComponent<CharacterPools>();
            if (characterPoolsManager == null)
                manager = poolGameObject.AddComponent<CharacterPools>();
        }
        Selection.activeObject = poolGameObject;
    }

    private void DrawCharacterCard(CharacterBase character)
    {
        if (character == null) return;

        CharacterData data = character.data;

        if (data == null) return;

        GUILayout.BeginVertical("box", GUILayout.Width(200), GUILayout.Height(150));
        Texture2D icon;
        if (data.turnUISprite != null)
            icon = data.turnUISprite.texture;
        else
            icon = Texture2D.grayTexture;

        Rect imageRect = GUILayoutUtility.GetRect(180, 100, GUILayout.ExpandWidth(false));
        GUI.DrawTexture(imageRect, icon, ScaleMode.ScaleAndCrop);

        string nameToShow = "(Unnamed)";
        nameToShow = character.name;
        
        GUILayout.Label(nameToShow, EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Select", GUILayout.Width(60)))
            Selection.activeObject = character;
        if (GUILayout.Button("Edit", GUILayout.Width(60)))
        {
            selectedCharacter = character;
            GUI.FocusControl(null);
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    private void CreateData()
    {
        if (database == null) { return; }
        CharacterData newData = CreateInstance<CharacterData>();
        int count = database.allCharacterDatas.Count;

        AssetDatabase.CreateAsset(newData, $"Assets/ScriptableData/Character/CharacterData({count}).asset");
        database.AddCharacterData(newData);
        selectedCharacterData = newData;

        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// IMGUI to draw character data card
    /// </summary>
    private void DrawDataCard(CharacterData data)
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();
        GUILayout.Label("Name: " + data.name, EditorStyles.boldLabel);
        GUILayout.Label("Type: " + data.type);
        GUILayout.Label("Unit: " + data.unitType);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Select", GUILayout.Width(60)))
            Selection.activeObject = data;
        if (GUILayout.Button("Edit", GUILayout.Width(50)))
        {
            selectedCharacterData = data;
            updateName = data.name;
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
                selectedCharacterData = null;

                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
}
