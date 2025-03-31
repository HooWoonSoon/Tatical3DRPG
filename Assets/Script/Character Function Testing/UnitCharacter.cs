using System;
using UnityEngine;

public class UnitCharacter : MonoBehaviour
{
    [SerializeField] string characterName;
    public Team team { get; set; }
}