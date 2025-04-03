using System;
using UnityEngine;

public class UnitCharacter : MonoBehaviour
{
    [SerializeField] string characterName;
    public int characterID;
    [HideInInspector] public bool isLink;
}