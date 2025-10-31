using UnityEditor;
using UnityEngine;
using System.Linq;

public enum UnitType
{
    Melee, Ranged
}

[CreateAssetMenu(fileName = "CharacterData", menuName = "Tactics/Character")]
public class CharacterData : ScriptableObject
{
    [Header("Character Information")]
    public string characterName;
    public Sprite characterProfile;
    public Sprite characterTurnUISprite;
    public int ID;
    public TeamType type;
    public UnitType unitType;

    [Header("Properties")]
    public int healthPoint;
    public int mentalPoint;
    public int magicAttackPoint;
    public int physicAttackPoint;
    public int speed;
    public int movableRange;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (ID == 0)
        {
            int maxID = AssetDatabase.FindAssets("t:CharacterData")
                .Select(guid => AssetDatabase.LoadAssetAtPath<CharacterData>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(c => c != null)
                .Select(c => c.ID)
                .DefaultIfEmpty(0)
                .Max();

            ID = maxID + 1;
            EditorUtility.SetDirty(this);
            Debug.Log($"Assigned new ID {ID} to {name}");
        }
        else
        {
            var duplicates = AssetDatabase.FindAssets("t:CharacterData")
                .Select(guid => AssetDatabase.LoadAssetAtPath<CharacterData>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(c => c != null && c != this && c.ID == ID)
                .ToList();

            if (duplicates.Count > 0)
            {
                Debug.LogError($"[CharacterData] Duplicate ID {ID} found in {duplicates[0].name} and {name}");
            }
        }
    }
#endif
}
