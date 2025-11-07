using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;
public class CharactersPoolEditor : EditorWindow
{
    private Texture2D bannerImage;
    private string searchFilter = "";
    private CharacterPoolsManager manager;
    private Vector2 scrollPos;
    private CharacterBase selectedCharacter;

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
        manager = GameObject.Find("Characters Pool").GetComponent<CharacterPoolsManager>();
        if (manager == null)
            Debug.LogWarning("Character pool not found. Make sure it's in Inpectors");
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

        if (GUILayout.Button("Refresh", GUILayout.Width(80), GUILayout.Height(35)))
        {
            LoadBannerImage();
            LoadCharacterPoolManager();
        }
        if (GUILayout.Button("Create Characters Pool", GUILayout.Width(160), GUILayout.Height(35)))
        {
            CreateCharacterPool();
        }
        if (GUILayout.Button("Select Character Pool", GUILayout.Width(160), GUILayout.Height(35)))
        {
            if (manager != null) Selection.activeObject = manager;
        }
        GUILayout.EndHorizontal();
        #endregion

        searchFilter = EditorGUILayout.TextField(searchFilter, EditorStyles.toolbarSearchField);

        #region UIRegion 2
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical("box", GUILayout.Width(215));
        GUILayout.Label("Character List", EditorStyles.boldLabel);
        scrollPos = GUILayout.BeginScrollView(scrollPos);

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
            int columns = Mathf.Max(1, Mathf.FloorToInt(position.width / 150));
            int index = 0;

            for (int i = 0; i < columns && index < list.Count; i++, index++)
            {
                DrawCharacterCard(list[index]);
            }
        }
        GUILayout.EndVertical();
        GUILayout.EndScrollView();

        GUILayout.BeginVertical("box");
        GUILayout.Label("Editor", EditorStyles.boldLabel);

        if (selectedCharacter != null)
        {
            CharacterEditorDrawer.DrawCharacterEditor(selectedCharacter);
        }
        else
        {
            EditorGUILayout.HelpBox("Press Edit button to select character to edit", MessageType.Info);
        }

        if (selectedCharacter.data != null)
        {
            CharacterEditorDrawer.DrawCharacterEditor(selectedCharacter.data);
        }
        else
        {
            EditorGUILayout.HelpBox("Missing character data", MessageType.Info);
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        #endregion
    }

    private void CreateCharacterPool()
    {
        GameObject poolGameObject = GameObject.Find("Characters Pool");
        if (poolGameObject == null)
            manager = new GameObject("Characters Pool").AddComponent<CharacterPoolsManager>();
        else
        {
            CharacterPoolsManager characterPoolsManager = poolGameObject.GetComponent<CharacterPoolsManager>();
            if (characterPoolsManager == null)
                manager = poolGameObject.AddComponent<CharacterPoolsManager>();
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
        if (data.characterName != null)
        {
            nameToShow = data.characterName;
        }
        GUILayout.Label(nameToShow, EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Select", GUILayout.Width(60)))
            Selection.activeObject = character;
        if (GUILayout.Button("Edit", GUILayout.Width(60)))
        {
            selectedCharacter = character;
        }
        EditorGUILayout.EndHorizontal();


        GUILayout.EndVertical();
    }
}
