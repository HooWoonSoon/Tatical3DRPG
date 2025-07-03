using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Character/Properties")]
public class CharacterData : ScriptableObject
{
    public enum Type
    {
        Enemy, Allay, Neutral
    }

    [Header("Character Information")]
    public string characterName;
    public Sprite characterProfile;
    public Sprite characterTurnUISprite;
    public int ID;
    public Type type;

    [Header("Properties")]
    public int healthPoint;
    public int mentalPoint;
    public int magicAttackPoint;
    public int physicAttackPoint;
    public int speed;
}
